using HarmonyPlugins.Api.Domain.Enums;

namespace HarmonyPlugins.Api.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; } = UserRole.Customer;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CustomerLibrary> LibraryItems { get; set; } = new List<CustomerLibrary>();
        public ICollection<DownloadLog> DownloadLogs { get; set; } = new List<DownloadLog>();
    }
}
