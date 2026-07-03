using HarmonyPlugins.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HarmonyPlugins.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<PluginCategory> PluginCategories => Set<PluginCategory>();

    public DbSet<Plugin> Plugins => Set<Plugin>();

    public DbSet<PluginVersion> PluginVersions => Set<PluginVersion>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<CustomerLibrary> CustomerLibraries => Set<CustomerLibrary>();

    public DbSet<DownloadLog> DownloadLogs => Set<DownloadLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigurePluginCategories(modelBuilder);
        ConfigurePlugins(modelBuilder);
        ConfigurePluginVersions(modelBuilder);
        ConfigureOrders(modelBuilder);
        ConfigureOrderItems(modelBuilder);
        ConfigurePayments(modelBuilder);
        ConfigureCustomerLibraries(modelBuilder);
        ConfigureDownloadLogs(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Name)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(180);

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.Property(user => user.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(user => user.Role)
                .IsRequired();

            entity.Property(user => user.CreatedAt)
                .IsRequired();
        });
    }

    private static void ConfigurePluginCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PluginCategory>(entity =>
        {
            entity.HasKey(category => category.Id);

            entity.Property(category => category.Name)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(category => category.Slug)
                .IsRequired()
                .HasMaxLength(160);

            entity.HasIndex(category => category.Slug)
                .IsUnique();

            entity.Property(category => category.Description)
                .HasMaxLength(500);

            entity.Property(category => category.IsActive)
                .IsRequired();

            entity.Property(category => category.CreatedAt)
                .IsRequired();
        });
    }

    private static void ConfigurePlugins(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Plugin>(entity =>
        {
            entity.HasKey(plugin => plugin.Id);

            entity.Property(plugin => plugin.Name)
                .IsRequired()
                .HasMaxLength(160);

            entity.Property(plugin => plugin.Slug)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(plugin => plugin.Slug)
                .IsUnique();

            entity.Property(plugin => plugin.ShortDescription)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(plugin => plugin.FullDescription)
                .IsRequired();

            entity.Property(plugin => plugin.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(plugin => plugin.Type)
                .IsRequired();

            entity.Property(plugin => plugin.Status)
                .IsRequired();

            entity.Property(plugin => plugin.BannerUrl)
                .HasMaxLength(500);

            entity.Property(plugin => plugin.CreatedAt)
                .IsRequired();

            entity.HasOne(plugin => plugin.Category)
                .WithMany(category => category.Plugins)
                .HasForeignKey(plugin => plugin.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePluginVersions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PluginVersion>(entity =>
        {
            entity.HasKey(version => version.Id);

            entity.Property(version => version.VersionNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(version => version.Changelog)
                .IsRequired();

            entity.Property(version => version.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(version => version.FileSize)
                .IsRequired();

            entity.Property(version => version.IsCurrent)
                .IsRequired();

            entity.Property(version => version.Status)
                .IsRequired();

            entity.Property(version => version.PublishedAt)
                .IsRequired();

            entity.Property(version => version.CreatedAt)
                .IsRequired();

            entity.HasOne(version => version.Plugin)
                .WithMany(plugin => plugin.Versions)
                .HasForeignKey(version => version.PluginId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(order => order.Id);

            entity.Property(order => order.Status)
                .IsRequired();

            entity.Property(order => order.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(order => order.CreatedAt)
                .IsRequired();

            entity.HasOne(order => order.User)
                .WithMany(user => user.Orders)
                .HasForeignKey(order => order.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOrderItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(item => item.Id);

            entity.Property(item => item.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(item => item.CreatedAt)
                .IsRequired();

            entity.HasOne(item => item.Order)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Plugin)
                .WithMany(plugin => plugin.OrderItems)
                .HasForeignKey(item => item.PluginId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePayments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(payment => payment.Id);

            entity.Property(payment => payment.Gateway)
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(payment => payment.GatewayPaymentId)
                .HasMaxLength(200);

            entity.Property(payment => payment.CheckoutUrl)
                .HasMaxLength(1000);

            entity.Property(payment => payment.Status)
                .IsRequired();

            entity.Property(payment => payment.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(payment => payment.CreatedAt)
                .IsRequired();

            entity.HasOne(payment => payment.Order)
                .WithOne(order => order.Payment)
                .HasForeignKey<Payment>(payment => payment.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCustomerLibraries(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerLibrary>(entity =>
        {
            entity.HasKey(library => library.Id);

            entity.Property(library => library.AccessType)
                .IsRequired();

            entity.Property(library => library.CreatedAt)
                .IsRequired();

            entity.HasIndex(library => new { library.UserId, library.PluginId })
                .IsUnique();

            entity.HasOne(library => library.User)
                .WithMany(user => user.LibraryItems)
                .HasForeignKey(library => library.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(library => library.Plugin)
                .WithMany(plugin => plugin.LibraryItems)
                .HasForeignKey(library => library.PluginId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(library => library.Order)
                .WithMany()
                .HasForeignKey(library => library.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureDownloadLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DownloadLog>(entity =>
        {
            entity.HasKey(download => download.Id);

            entity.Property(download => download.IpAddress)
                .HasMaxLength(80);

            entity.Property(download => download.DownloadedAt)
                .IsRequired();

            entity.HasOne(download => download.User)
                .WithMany(user => user.DownloadLogs)
                .HasForeignKey(download => download.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(download => download.Plugin)
                .WithMany(plugin => plugin.DownloadLogs)
                .HasForeignKey(download => download.PluginId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(download => download.PluginVersion)
                .WithMany(version => version.DownloadLogs)
                .HasForeignKey(download => download.PluginVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}