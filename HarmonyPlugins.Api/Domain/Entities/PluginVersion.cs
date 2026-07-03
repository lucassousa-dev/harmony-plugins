using HarmonyPlugins.Api.Domain.Enums;

namespace HarmonyPlugins.Api.Domain.Entities
{
    public class PluginVersion
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PluginId { get; set; }
        public Plugin? Plugin { get; set; }
        public string VersionNumber { get; set; }
        public string Changelog { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public bool IsCurrent { get; set; }
        public PluginVersionStatus Status { get; set; } = PluginVersionStatus.Active;
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<DownloadLog> DownloadLogs { get; set; } = new List<DownloadLog>();
    }
}
