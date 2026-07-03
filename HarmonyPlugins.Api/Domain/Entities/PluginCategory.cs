namespace HarmonyPlugins.Api.Domain.Entities
{
    public class PluginCategory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Slug { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Plugin> Plugins { get; set; } = new List<Plugin>();
    }
}
