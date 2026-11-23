using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Domain.Entities;
using HarmonyPlugins.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HarmonyPlugins.Api.Infrastructure.Data;

// RN16: o cadastro público só cria clientes; o primeiro administrador é criado na implantação,
// a partir de Admin:Email e Admin:Password.
public static class AdminSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext, IPasswordService passwordService, IConfiguration configuration)
    {
        var email = configuration["Admin:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["Admin:Password"];

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return;

        if (await dbContext.Users.AnyAsync(u => u.Role == UserRole.Admin))
            return;

        if (password.Length is < 15 or > 64)
            throw new InvalidOperationException("A senha do administrador inicial precisa ter de 15 a 64 caracteres.");

        // Não promove conta existente: alguém poderia se cadastrar antes com o e-mail do administrador.
        if (await dbContext.Users.AnyAsync(u => u.Email == email))
            throw new InvalidOperationException("O e-mail do administrador inicial já pertence a outra conta.");

        dbContext.Users.Add(new User
        {
            Name = configuration["Admin:Name"] ?? "Administrador",
            Email = email,
            PasswordHash = passwordService.HashPassword(password),
            Role = UserRole.Admin,
            Status = UserStatus.Active
        });
        await dbContext.SaveChangesAsync();
    }
}
