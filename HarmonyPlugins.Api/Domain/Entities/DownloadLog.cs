namespace HarmonyPlugins.Api.Domain.Entities
{
    public class DownloadLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public Guid PluginId { get; set; }
        public Plugin? Plugin { get; set; }
        public Guid PluginVersionId { get; set; }
        public PluginVersion? PluginVersion { get; set; }
        public string? IpAddress { get; set; }
        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }
}
