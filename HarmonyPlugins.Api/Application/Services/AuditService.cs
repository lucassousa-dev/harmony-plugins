using System.Security.Claims;
using System.Text.Json;
using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Domain.Entities;
using HarmonyPlugins.Api.Infrastructure.Data;

namespace HarmonyPlugins.Api.Application.Services;

public class AuditService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor) : IAuditService
{
    public void Record(string action, string resource, object? resourceId = null, Guid? userId = null, object? details = null)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (userId is null && Guid.TryParse(httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId))
            userId = currentUserId;

        dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            Resource = resource,
            ResourceId = resourceId?.ToString(),
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            Details = details is null ? null : JsonSerializer.Serialize(details)
        });
    }
}
