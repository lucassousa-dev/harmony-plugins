using System.Security.Cryptography;
using System.Text.RegularExpressions;
using HarmonyPlugins.Api.Application.Common;
using HarmonyPlugins.Api.Application.DTOs.Plugins;
using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Domain.Entities;
using HarmonyPlugins.Api.Domain.Enums;
using HarmonyPlugins.Api.Domain.Exceptions;
using HarmonyPlugins.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HarmonyPlugins.Api.Application.Services;

public partial class PluginService(AppDbContext dbContext, IAuditService auditService, IFileStorage fileStorage)
    : IPluginService
{
    // RNF04: só .zip e .jar, até 100 MB.
    public const long MaxFileSize = 100L * 1024 * 1024;
    private static readonly string[] AllowedExtensions = [".zip", ".jar"];

    // RN06: versionamento semântico (MAJOR.MINOR.PATCH, com pré-lançamento e build opcionais).
    [GeneratedRegex(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(-[0-9A-Za-z.-]+)?(\+[0-9A-Za-z.-]+)?$")]
    private static partial Regex SemanticVersion();

    public async Task<List<PluginResponse>> ListAsync()
    {
        var plugins = await dbContext.Plugins.AsNoTracking()
            .Include(p => p.Game)
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return plugins.Select(p => ToResponse(p)).ToList();
    }

    public async Task<PluginResponse> GetAsync(Guid id)
    {
        var plugin = await dbContext.Plugins.AsNoTracking()
            .Include(p => p.Game)
            .Include(p => p.Category)
            .Include(p => p.Versions)
            .SingleOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Plugin não encontrado.");

        return ToResponse(plugin, withVersions: true);
    }

    public async Task<PluginResponse> CreateAsync(PluginRequest request)
    {
        var plugin = new Plugin { Status = PluginStatus.Draft };
        await ApplyAsync(plugin, request);
        dbContext.Plugins.Add(plugin);
        auditService.Record("CRIAR_PLUGIN", "Plugin", plugin.Id);
        await dbContext.SaveChangesAsync();
        return await GetAsync(plugin.Id);
    }

    public async Task<PluginResponse> UpdateAsync(Guid id, PluginRequest request)
    {
        var plugin = await FindAsync(id);
        var oldPrice = plugin.Price;

        await ApplyAsync(plugin, request);
        plugin.UpdatedAt = DateTime.UtcNow;

        auditService.Record("EDITAR_PLUGIN", "Plugin", plugin.Id);
        if (plugin.Price != oldPrice)
            auditService.Record("ALTERAR_PRECO", "Plugin", plugin.Id, details: new { de = oldPrice, para = plugin.Price });

        await dbContext.SaveChangesAsync();
        return await GetAsync(plugin.Id);
    }

    public async Task<PluginResponse> PublishAsync(Guid id)
    {
        var plugin = await FindAsync(id);

        // Suspender e reativar é moderação (UC10), não publicação.
        if (plugin.Status == PluginStatus.Inactive)
            throw new BusinessRuleException("Plugin suspenso não pode ser publicado. Reative-o pela moderação.");

        // RN09: só publica com uma versão atual ativa.
        if (!await dbContext.PluginVersions.AnyAsync(v =>
                v.PluginId == id && v.IsCurrent && v.Status == PluginVersionStatus.Active))
            throw new BusinessRuleException("O plugin precisa de uma versão atual ativa para ser publicado.");

        plugin.Status = PluginStatus.Published;
        plugin.UpdatedAt = DateTime.UtcNow;
        auditService.Record("PUBLICAR_PLUGIN", "Plugin", plugin.Id);
        await dbContext.SaveChangesAsync();
        return await GetAsync(id);
    }

    public async Task<PluginResponse> UnpublishAsync(Guid id)
    {
        var plugin = await FindAsync(id);

        if (plugin.Status == PluginStatus.Inactive)
            throw new BusinessRuleException("Plugin suspenso não pode ser despublicado. Reative-o pela moderação.");

        // Aquisições existentes são mantidas (UC07).
        plugin.Status = PluginStatus.Draft;
        plugin.UpdatedAt = DateTime.UtcNow;
        auditService.Record("DESPUBLICAR_PLUGIN", "Plugin", plugin.Id);
        await dbContext.SaveChangesAsync();
        return await GetAsync(id);
    }

    public async Task<PluginVersionResponse> AddVersionAsync(
        Guid pluginId, PluginVersionRequest request, CancellationToken cancellationToken = default)
    {
        await FindAsync(pluginId);

        var versionNumber = request.VersionNumber.Trim();
        if (!SemanticVersion().IsMatch(versionNumber))
            throw new BusinessRuleException("Use o versionamento semântico no número da versão, por exemplo 1.4.2.");

        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new BusinessRuleException("Somente arquivos .zip ou .jar são aceitos.");

        if (request.File.Length is <= 0 or > MaxFileSize)
            throw new BusinessRuleException("O arquivo precisa ter entre 1 byte e 100 MB.");

        if (await dbContext.PluginVersions.AnyAsync(v => v.PluginId == pluginId && v.VersionNumber == versionNumber, cancellationToken))
            throw new ConflictException("Essa versão já existe neste plugin.");

        string sha256;
        await using (var stream = request.File.OpenReadStream())
            sha256 = Convert.ToHexStringLower(await SHA256.HashDataAsync(stream, cancellationToken));

        var version = new PluginVersion
        {
            PluginId = pluginId,
            VersionNumber = versionNumber,
            Changelog = request.Changelog.Trim(),
            Compatibility = request.Compatibility.Trim(),
            FileSize = request.File.Length,
            FileSha256 = sha256,
            Status = PluginVersionStatus.Active
        };
        version.FilePath = $"{pluginId}/{version.Id}{extension}";

        // UC08 5b: se o armazenamento falhar, a exceção sobe e nada é gravado no banco.
        await using (var stream = request.File.OpenReadStream())
            await fileStorage.UploadAsync(version.FilePath, stream, cancellationToken);

        try
        {
            dbContext.PluginVersions.Add(version);
            auditService.Record("PUBLICAR_VERSAO", "PluginVersion", version.Id,
                details: new { pluginId, versionNumber, sha256 });

            if (request.IsCurrent)
                await MakeCurrentAsync(version);
            else
                await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Sem registro no banco, o arquivo ficaria órfão no contêiner.
            await fileStorage.DeleteAsync(version.FilePath, CancellationToken.None);
            throw;
        }

        return ToResponse(version);
    }

    public async Task<PluginVersionResponse> SetCurrentVersionAsync(Guid pluginId, Guid versionId)
    {
        var version = await dbContext.PluginVersions.SingleOrDefaultAsync(v => v.Id == versionId && v.PluginId == pluginId)
            ?? throw new NotFoundException("Versão não encontrada.");

        // RN13: a versão atual precisa estar ativa.
        if (version.Status != PluginVersionStatus.Active)
            throw new BusinessRuleException("Somente uma versão ativa pode ser a versão atual.");

        if (!version.IsCurrent)
        {
            auditService.Record("DEFINIR_VERSAO_ATUAL", "PluginVersion", version.Id, details: new { pluginId });
            await MakeCurrentAsync(version);
        }

        return ToResponse(version);
    }

    // RN13: o índice único filtrado aceita só uma versão atual por plugin. A anterior é desmarcada
    // em um SaveChanges separado, porque o EF não garante a ordem dos UPDATEs dentro de um só.
    private async Task MakeCurrentAsync(PluginVersion version)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var previous = await dbContext.PluginVersions
            .Where(v => v.PluginId == version.PluginId && v.IsCurrent && v.Id != version.Id)
            .ToListAsync();
        foreach (var old in previous)
        {
            old.IsCurrent = false;
            old.UpdatedAt = DateTime.UtcNow;
        }
        await dbContext.SaveChangesAsync();

        version.IsCurrent = true;
        version.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    private async Task ApplyAsync(Plugin plugin, PluginRequest request)
    {
        // RN09: gratuito custa zero; pago custa mais que zero.
        if (request.Type == PluginType.Free ? request.Price != 0 : request.Price <= 0)
            throw new BusinessRuleException("Plugin gratuito deve ter preço zero, e plugin pago, preço maior que zero.");

        if (!await dbContext.Games.AnyAsync(g => g.Id == request.GameId))
            throw new BusinessRuleException("Jogo não encontrado.");

        if (!await dbContext.PluginCategories.AnyAsync(c => c.Id == request.CategoryId))
            throw new BusinessRuleException("Categoria não encontrada.");

        var name = request.Name.Trim();
        var slug = Slug.From(name);
        if (slug.Length == 0)
            throw new BusinessRuleException("O nome precisa ter ao menos uma letra ou número.");

        if (await dbContext.Plugins.AnyAsync(p => p.Slug == slug && p.Id != plugin.Id))
            throw new ConflictException("Já existe um plugin com esse nome.");

        plugin.Name = name;
        plugin.Slug = slug;
        plugin.ShortDescription = request.ShortDescription.Trim();
        plugin.FullDescription = request.FullDescription.Trim();
        plugin.GameId = request.GameId;
        plugin.CategoryId = request.CategoryId;
        plugin.Type = request.Type;
        plugin.Price = request.Price;
        plugin.BannerUrl = string.IsNullOrWhiteSpace(request.BannerUrl) ? null : request.BannerUrl.Trim();
    }

    private async Task<Plugin> FindAsync(Guid id) =>
        await dbContext.Plugins.FindAsync(id) ?? throw new NotFoundException("Plugin não encontrado.");

    private static PluginResponse ToResponse(Plugin plugin, bool withVersions = false) => new()
    {
        Id = plugin.Id,
        Name = plugin.Name,
        Slug = plugin.Slug,
        ShortDescription = plugin.ShortDescription,
        FullDescription = plugin.FullDescription,
        Type = plugin.Type,
        Price = plugin.Price,
        Status = plugin.Status,
        SuspensionReason = plugin.SuspensionReason,
        BannerUrl = plugin.BannerUrl,
        GameId = plugin.GameId,
        GameName = plugin.Game?.Name ?? "",
        CategoryId = plugin.CategoryId,
        CategoryName = plugin.Category?.Name ?? "",
        CreatedAt = plugin.CreatedAt,
        UpdatedAt = plugin.UpdatedAt,
        Versions = withVersions
            ? plugin.Versions.OrderByDescending(v => v.PublishedAt).Select(ToResponse).ToList()
            : []
    };

    internal static PluginVersionResponse ToResponse(PluginVersion version) => new()
    {
        Id = version.Id,
        VersionNumber = version.VersionNumber,
        Changelog = version.Changelog,
        Compatibility = version.Compatibility,
        FileSize = version.FileSize,
        FileSha256 = version.FileSha256,
        IsCurrent = version.IsCurrent,
        Status = version.Status,
        PublishedAt = version.PublishedAt
    };
}
