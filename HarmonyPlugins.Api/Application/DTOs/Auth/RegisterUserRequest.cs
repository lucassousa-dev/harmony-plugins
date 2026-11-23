using System.ComponentModel.DataAnnotations;

namespace HarmonyPlugins.Api.Application.DTOs.Auth
{
    public class RegisterUserRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(120)]
        public string Name { get; set; }

        [Required(ErrorMessage = "O email é obrigatório.")]
        [MaxLength(180, ErrorMessage = "O email pode ter no máximo 180 caracteres.")]
        [EmailAddress(ErrorMessage = "O email informado é inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        // RNF01 (NIST SP 800-63B-4): 15 a 64 caracteres, sem regras de composição.
        [MinLength(15, ErrorMessage = "A senha precisa ter no mínimo 15 caracteres.")]
        [MaxLength(64, ErrorMessage = "A senha pode ter no máximo 64 caracteres.")]
        public string Password { get; set; }
    }
}
