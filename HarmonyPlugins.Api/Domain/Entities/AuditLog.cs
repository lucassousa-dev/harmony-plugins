namespace HarmonyPlugins.Api.Domain.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }
        public string? ResourceId { get; set; }
        public string? IpAddress { get; set; }
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
