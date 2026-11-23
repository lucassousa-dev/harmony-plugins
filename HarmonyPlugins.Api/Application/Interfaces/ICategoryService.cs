using HarmonyPlugins.Api.Application.DTOs.Categories;

namespace HarmonyPlugins.Api.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryResponse>> ListAsync(bool onlyActive);

    Task<CategoryResponse> CreateAsync(CategoryRequest request);

    Task<CategoryResponse> UpdateAsync(Guid id, CategoryRequest request);
}
