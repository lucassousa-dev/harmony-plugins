using HarmonyPlugins.Api.Application.Common;
using HarmonyPlugins.Api.Application.DTOs.Categories;
using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Domain.Entities;
using HarmonyPlugins.Api.Domain.Exceptions;
using HarmonyPlugins.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HarmonyPlugins.Api.Application.Services;

public class CategoryService(AppDbContext dbContext, IAuditService auditService) : ICategoryService
{
    public Task<List<CategoryResponse>> ListAsync(bool onlyActive) =>
        dbContext.PluginCategories.AsNoTracking()
            .Where(c => !onlyActive || c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync();

    public async Task<CategoryResponse> CreateAsync(CategoryRequest request)
    {
        var category = new PluginCategory();
        await ApplyAsync(category, request);
        dbContext.PluginCategories.Add(category);
        auditService.Record("CRIAR_CATEGORIA", "PluginCategory", category.Id);
        await dbContext.SaveChangesAsync();
        return ToResponse(category);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, CategoryRequest request)
    {
        var category = await dbContext.PluginCategories.FindAsync(id)
            ?? throw new NotFoundException("Categoria não encontrada.");
        await ApplyAsync(category, request);
        category.UpdatedAt = DateTime.UtcNow;
        auditService.Record("EDITAR_CATEGORIA", "PluginCategory", category.Id);
        await dbContext.SaveChangesAsync();
        return ToResponse(category);
    }

    private async Task ApplyAsync(PluginCategory category, CategoryRequest request)
    {
        var name = request.Name.Trim();
        var slug = Slug.From(name);
        if (slug.Length == 0)
            throw new BusinessRuleException("O nome precisa ter ao menos uma letra ou número.");

        if (await dbContext.PluginCategories.AnyAsync(c => (c.Name == name || c.Slug == slug) && c.Id != category.Id))
            throw new ConflictException("Já existe uma categoria com esse nome.");

        category.Name = name;
        category.Slug = slug;
        category.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        category.IsActive = request.IsActive;
    }

    private static CategoryResponse ToResponse(PluginCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Slug = category.Slug,
        Description = category.Description,
        IsActive = category.IsActive
    };
}
