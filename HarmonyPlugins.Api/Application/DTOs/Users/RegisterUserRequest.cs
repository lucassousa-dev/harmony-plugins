using System.ComponentModel.DataAnnotations;

namespace HarmonyPlugins.Api.Application.DTOs.Users
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
        [MinLength(6, ErrorMessage = "A senha precisa ter no mínimo 6 caracteres.")]
        [MaxLength(100, ErrorMessage = "A senha pode ter no máximo 100 caracteres.")]
        public string Password { get; set; }
    }
}
