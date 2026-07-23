using System.ComponentModel.DataAnnotations;

namespace HarmonyPlugins.Api.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O email informado é inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Password { get; set; }
    }
}
