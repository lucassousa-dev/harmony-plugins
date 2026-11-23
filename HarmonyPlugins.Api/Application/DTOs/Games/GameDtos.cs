using System.ComponentModel.DataAnnotations;

namespace HarmonyPlugins.Api.Application.DTOs.Games;

public class GameRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
    public string Name { get; set; }

    public bool IsActive { get; set; } = true;
}

public class GameResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
}
