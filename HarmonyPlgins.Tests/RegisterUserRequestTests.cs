using System.ComponentModel.DataAnnotations;
using HarmonyPlugins.Api.Application.DTOs.Auth;

namespace HarmonyPlgins.Tests
{
    public class RegisterUserRequestTests
    {
        // RNF01: senha de 15 a 64 caracteres.
        [Theory]
        [InlineData(14, false)]
        [InlineData(15, true)]
        [InlineData(64, true)]
        [InlineData(65, false)]
        public void Password_LengthRule(int length, bool expectedValid)
        {
            var request = new RegisterUserRequest
            {
                Name = "Cliente",
                Email = "cliente@example.com",
                Password = new string('a', length)
            };

            var isValid = Validator.TryValidateObject(
                request, new ValidationContext(request), new List<ValidationResult>(), validateAllProperties: true);

            Assert.Equal(expectedValid, isValid);
        }
    }
}
