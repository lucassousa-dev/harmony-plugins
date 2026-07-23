using HarmonyPlugins.Api.Application.Interfaces;

namespace HarmonyPlugins.Api.Application.Services
{
    public class PasswordService : IPasswordService
    {

        public string HashPassword(string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
          
            return passwordHash;
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, passwordHash);

            return isValidPassword;
        }
    }
}
