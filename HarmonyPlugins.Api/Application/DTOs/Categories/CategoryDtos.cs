using System.ComponentModel.DataAnnotations;

namespace HarmonyPlugins.Api.Application.DTOs.Categories;

public class CategoryRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(120, ErrorMessage = "O nome pode ter no máximo 120 caracteres.")]
    public string Name { get; set; }

    [MaxLength(500, ErrorMessage = "A descrição pode ter no máximo 500 caracteres.")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
