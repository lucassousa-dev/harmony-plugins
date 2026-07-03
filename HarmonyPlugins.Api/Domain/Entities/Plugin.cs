using HarmonyPlugins.Api.Domain.Enums;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HarmonyPlugins.Api.Domain.Entities
{
    public class Plugin
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Slug { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public decimal Price { get; set; }
        public PluginType Type { get; set; }
        public PluginStatus Status { get; set; } = PluginStatus.Draft;
        public string? BannerUrl { get; set; }
        public Guid CategoryId { get; set; }
        public PluginCategory? Category { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<PluginVersion> Versions { get; set; } = new List<PluginVersion>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CustomerLibrary> LibraryItems { get; set; } = new List<CustomerLibrary>();
        public ICollection<DownloadLog> DownloadLogs { get; set; } = new List<DownloadLog>();
    }
}
