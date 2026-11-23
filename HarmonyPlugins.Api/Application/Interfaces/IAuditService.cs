namespace HarmonyPlugins.Api.Application.Interfaces;

public interface IAuditService
{
    // Só adiciona ao contexto: o registro é gravado no mesmo SaveChanges da operação auditada.
    void Record(string action, string resource, object? resourceId = null, Guid? userId = null, object? details = null);
}
