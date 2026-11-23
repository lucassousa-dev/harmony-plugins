using System.ComponentModel.DataAnnotations;
using HarmonyPlugins.Api.Application.DTOs.Plugins;
using HarmonyPlugins.Api.Domain.Enums;

namespace HarmonyPlugins.Api.Application.DTOs.Catalog;

public class CatalogQuery
{
    [MaxLength(160)]
    public string? Search { get; set; }

    public Guid? GameId { get; set; }

    public Guid? CategoryId { get; set; }

    [EnumDataType(typeof(PluginType), ErrorMessage = "Tipo inválido.")]
    public PluginType? Type { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Preço mínimo inválido.")]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Preço máximo inválido.")]
    public decimal? MaxPrice { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A página começa em 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "O tamanho da página vai de 1 a 100.")]
    public int PageSize { get; set; } = 20;
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class CatalogPluginResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string ShortDescription { get; set; }
    public PluginType Type { get; set; }
    public decimal Price { get; set; }
    public string? BannerUrl { get; set; }
    public string GameName { get; set; }
    public string CategoryName { get; set; }
    public string? CurrentVersion { get; set; }
}

public class CatalogPluginDetailResponse : CatalogPluginResponse
{
    public string FullDescription { get; set; }
    public string? Compatibility { get; set; }
    public List<PluginVersionResponse> Versions { get; set; } = [];
}
