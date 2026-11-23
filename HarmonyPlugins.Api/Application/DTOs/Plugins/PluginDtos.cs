using System.ComponentModel.DataAnnotations;
using HarmonyPlugins.Api.Domain.Enums;

namespace HarmonyPlugins.Api.Application.DTOs.Plugins;

public class PluginRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(160, ErrorMessage = "O nome pode ter no máximo 160 caracteres.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "O resumo é obrigatório.")]
    [MaxLength(300, ErrorMessage = "O resumo pode ter no máximo 300 caracteres.")]
    public string ShortDescription { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    public string FullDescription { get; set; }

    public Guid GameId { get; set; }

    public Guid CategoryId { get; set; }

    [EnumDataType(typeof(PluginType), ErrorMessage = "Tipo inválido.")]
    public PluginType Type { get; set; }

    [Range(0, 999_999.99, ErrorMessage = "Preço inválido.")]
    public decimal Price { get; set; }

    [MaxLength(500)]
    [Url(ErrorMessage = "Informe um endereço válido para a imagem de capa.")]
    public string? BannerUrl { get; set; }
}

public class PluginResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string ShortDescription { get; set; }
    public string FullDescription { get; set; }
    public PluginType Type { get; set; }
    public decimal Price { get; set; }
    public PluginStatus Status { get; set; }
    public string? SuspensionReason { get; set; }
    public string? BannerUrl { get; set; }
    public Guid GameId { get; set; }
    public string GameName { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PluginVersionResponse> Versions { get; set; } = [];
}

public class PluginVersionRequest
{
    [Required(ErrorMessage = "O número da versão é obrigatório.")]
    [MaxLength(50)]
    public string VersionNumber { get; set; }

    [Required(ErrorMessage = "As notas de alteração são obrigatórias.")]
    public string Changelog { get; set; }

    [Required(ErrorMessage = "A compatibilidade é obrigatória.")]
    [MaxLength(255, ErrorMessage = "A compatibilidade pode ter no máximo 255 caracteres.")]
    public string Compatibility { get; set; }

    public bool IsCurrent { get; set; }

    [Required(ErrorMessage = "O arquivo é obrigatório.")]
    public IFormFile File { get; set; }
}

public class PluginVersionResponse
{
    public Guid Id { get; set; }
    public string VersionNumber { get; set; }
    public string Changelog { get; set; }
    public string Compatibility { get; set; }
    public long FileSize { get; set; }
    public string FileSha256 { get; set; }
    public bool IsCurrent { get; set; }
    public PluginVersionStatus Status { get; set; }
    public DateTime PublishedAt { get; set; }
}
