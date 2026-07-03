using HarmonyPlugins.Api.Domain.Enums;

namespace HarmonyPlugins.Api.Domain.Entities
{
    public class CustomerLibrary
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public Guid PluginId { get; set; }
        public Plugin? Plugin { get; set; }
        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }
        public LibraryAccessType AccessType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
