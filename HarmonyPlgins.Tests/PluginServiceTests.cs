using HarmonyPlugins.Api.Application.DTOs.Plugins;
using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Application.Services;
using HarmonyPlugins.Api.Domain.Enums;
using HarmonyPlugins.Api.Domain.Exceptions;
using HarmonyPlugins.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HarmonyPlgins.Tests;

public class PluginServiceTests
{
    [Theory]
    [InlineData(PluginType.Free, 10)]
    [InlineData(PluginType.Paid, 0)]
    public async Task CreateAsync_WithPriceIncompatibleWithType_ThrowsBusinessRule(PluginType type, decimal price)
    {
        await using var dbContext = TestDb.Create();
        var (service, request, _) = await ArrangeAsync(dbContext);
        request.Type = type;
        request.Price = price;

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_CreatesDraftWithSlugAndAudit()
    {
        await using var dbContext = TestDb.Create();
        var (service, request, _) = await ArrangeAsync(dbContext);

        var plugin = await service.CreateAsync(request);

        Assert.Equal(PluginStatus.Draft, plugin.Status);
        Assert.Equal("loja-de-itens", plugin.Slug);
        Assert.Contains(dbContext.AuditLogs, log => log.Action == "CRIAR_PLUGIN");
    }

    [Fact]
    public async Task PublishAsync_WithoutCurrentVersion_ThrowsBusinessRule()
    {
        await using var dbContext = TestDb.Create();
        var (service, request, _) = await ArrangeAsync(dbContext);
        var plugin = await service.CreateAsync(request);

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.PublishAsync(plugin.Id));
    }

    [Fact]
    public async Task AddVersionAsync_StoresFileWithHashAndAllowsPublishing()
    {
        await using var dbContext = TestDb.Create();
        var (service, request, storage) = await ArrangeAsync(dbContext);
        var plugin = await service.CreateAsync(request);

        var version = await service.AddVersionAsync(plugin.Id, VersionRequest("1.0.0", isCurrent: true));
        var published = await service.PublishAsync(plugin.Id);

        Assert.True(version.IsCurrent);
        Assert.Equal(64, version.FileSha256.Length);
        Assert.Single(storage.Uploaded);
        Assert.Equal(PluginStatus.Published, published.Status);
    }

    [Fact]
    public async Task AddVersionAsync_AsCurrent_UnmarksThePreviousCurrentVersion()
    {
        await using var dbContext = TestDb.Create();
        var (service, request, _) = await ArrangeAsync(dbContext);
        var plugin = await service.CreateAsync(request);
        var first = await service.AddVersionAsync(plugin.Id, VersionRequest("1.0.0", isCurrent: true));

        var second = await service.AddVersionAsync(plugin.Id, VersionRequest("1.1.0", isCurrent: true));

        var current = await dbContext.PluginVersions.Where(v => v.IsCurrent).ToListAsync();
        Assert.Equal(second.Id, Assert.Single(current).Id);
        Assert.Equal(2, await dbContext.PluginVersions.CountAsync());
    }

    [Fact]
    public async Task AddVersionAsync_WithRepeatedNumber_ThrowsConflict()
    {
        await using var dbContext = TestDb.Create();
        var (service, request, _) = await ArrangeAsync(dbContext);
        var plugin = await service.CreateAsync(request);
        await service.AddVersionAsync(plugin.Id, VersionRequest("1.0.0"));

        await Assert.ThrowsAsync<ConflictException>(() => service.AddVersionAsync(plugin.Id, VersionRequest("1.0.0")));
    }

    [Theory]
    [InlineData("1.0", "plugin.zip")]
    [InlineData("1.0.0", "plugin.dll")]
    public async Task AddVersionAsync_WithInvalidNumberOrExtension_ThrowsBusinessRuleWithoutUploading(string number, string fileName)
    {
        await using var dbContext = TestDb.Create();
        var (service, request, storage) = await ArrangeAsync(dbContext);
        var plugin = await service.CreateAsync(request);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.AddVersionAsync(plugin.Id, VersionRequest(number, fileName: fileName)));
        Assert.Empty(storage.Uploaded);
    }

    private static async Task<(PluginService, PluginRequest, FakeFileStorage)> ArrangeAsync(AppDbContext dbContext)
    {
        var (game, category) = await TestDb.SeedGameAndCategoryAsync(dbContext);
        var storage = new FakeFileStorage();
        var service = new PluginService(dbContext, new AuditService(dbContext, new HttpContextAccessor()), storage);
        var request = new PluginRequest
        {
            Name = "Loja de Itens",
            ShortDescription = "Resumo",
            FullDescription = "Descrição completa",
            GameId = game.Id,
            CategoryId = category.Id,
            Type = PluginType.Paid,
            Price = 19.90m
        };
        return (service, request, storage);
    }

    private static PluginVersionRequest VersionRequest(string number, bool isCurrent = false, string fileName = "plugin.zip")
    {
        var content = new MemoryStream("conteudo do plugin"u8.ToArray());
        return new PluginVersionRequest
        {
            VersionNumber = number,
            Changelog = "Primeira versão",
            Compatibility = "1.20 a 1.21",
            IsCurrent = isCurrent,
            File = new FormFile(content, 0, content.Length, "File", fileName)
        };
    }

    private sealed class FakeFileStorage : IFileStorage
    {
        public List<string> Uploaded { get; } = [];

        public Task UploadAsync(string path, Stream content, CancellationToken cancellationToken = default)
        {
            Uploaded.Add(path);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            Uploaded.Remove(path);
            return Task.CompletedTask;
        }
    }
}
