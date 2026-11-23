using System.ComponentModel.DataAnnotations;

namespace HarmonyPlugins.Api.Application.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "O token de renovação é obrigatório.")]
    public string RefreshToken { get; set; }
}
