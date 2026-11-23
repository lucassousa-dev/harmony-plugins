using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Application.Services;
using HarmonyPlugins.Api.Infrastructure.Data;
using HarmonyPlugins.Api.Infrastructure.Storage;
using HarmonyPlugins.Api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IPasswordService, PasswordService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPluginService, PluginService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddSingleton<IFileStorage, BlobFileStorage>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("A chave JWT não foi configurada.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("O emissor JWT não foi configurado.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("A audiência JWT não foi configurada.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("A chave JWT deve ter pelo menos 32 bytes.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// RNF02: negação como padrão. Rota pública precisa de [AllowAnonymous].
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// RNF03: 5 tentativas de login por minuto por IP e 100 requisições por minuto por usuário autenticado.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        context.User.FindFirstValue(ClaimTypes.NameIdentifier) is { } userId
            ? RateLimitPartition.GetFixedWindowLimiter(userId,
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) })
            : RateLimitPartition.GetNoLimiter("anonimo"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Ligado no Docker Compose para o ambiente subir com um único comando (RNF10).
    if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
        dbContext.Database.Migrate();

    await AdminSeeder.SeedAsync(dbContext, scope.ServiceProvider.GetRequiredService<IPasswordService>(), app.Configuration);
}

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Harmony Plugins API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
// Depois da autenticação, para o limite global conhecer o usuário.
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

app.Run();
