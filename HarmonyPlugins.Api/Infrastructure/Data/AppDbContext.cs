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

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Game> Games => Set<Game>();

    public DbSet<PluginCategory> PluginCategories => Set<PluginCategory>();

    public DbSet<Plugin> Plugins => Set<Plugin>();

    public DbSet<PluginVersion> PluginVersions => Set<PluginVersion>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<CustomerLibrary> CustomerLibraries => Set<CustomerLibrary>();

    public DbSet<DownloadLog> DownloadLogs => Set<DownloadLog>();

    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureRefreshTokens(modelBuilder);
        ConfigureGames(modelBuilder);
        ConfigurePluginCategories(modelBuilder);
        ConfigurePlugins(modelBuilder);
        ConfigurePluginVersions(modelBuilder);
        ConfigureOrders(modelBuilder);
        ConfigureOrderItems(modelBuilder);
        ConfigurePayments(modelBuilder);
        ConfigureCustomerLibraries(modelBuilder);
        ConfigureDownloadLogs(modelBuilder);
        ConfigureWebhookEvents(modelBuilder);
        ConfigureAuditLogs(modelBuilder);
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

            entity.Property(user => user.Status)
                .IsRequired();

            entity.Property(user => user.CreatedAt)
                .IsRequired();

            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Users_Role", "[Role] IN (1, 2)");
                table.HasCheckConstraint("CK_Users_Status", "[Status] IN (1, 2)");
            });
        });
    }

    private static void ConfigureRefreshTokens(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(token => token.Id);

            entity.Property(token => token.TokenHash)
                .IsRequired()
                .HasMaxLength(64)
                .IsFixedLength()
                .IsUnicode(false);

            entity.HasIndex(token => token.TokenHash)
                .IsUnique();

            entity.Property(token => token.ExpiresAt)
                .IsRequired();

            entity.Property(token => token.CreatedAt)
                .IsRequired();

            entity.HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureGames(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(game => game.Id);

            entity.Property(game => game.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(game => game.Name)
                .IsUnique();

            entity.Property(game => game.IsActive)
                .IsRequired();

            entity.Property(game => game.CreatedAt)
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

            entity.Property(plugin => plugin.SuspensionReason)
                .HasMaxLength(255);

            entity.Property(plugin => plugin.BannerUrl)
                .HasMaxLength(500);

            entity.Property(plugin => plugin.CreatedAt)
                .IsRequired();

            entity.HasOne(plugin => plugin.Category)
                .WithMany(category => category.Plugins)
                .HasForeignKey(plugin => plugin.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(plugin => plugin.Game)
                .WithMany(game => game.Plugins)
                .HasForeignKey(plugin => plugin.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Plugins_Type", "[Type] IN (1, 2)");
                table.HasCheckConstraint("CK_Plugins_Status", "[Status] IN (1, 2, 3)");
                // RN09: gratuito custa 0, pago custa mais que 0.
                table.HasCheckConstraint(
                    "CK_Plugins_Price",
                    "([Type] = 1 AND [Price] = 0) OR ([Type] = 2 AND [Price] > 0)");
            });
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

            entity.Property(version => version.Compatibility)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(version => version.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(version => version.FileSize)
                .IsRequired();

            entity.Property(version => version.FileSha256)
                .IsRequired()
                .HasMaxLength(64)
                .IsFixedLength()
                .IsUnicode(false);

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

            // RN06: número da versão único dentro do plugin.
            entity.HasIndex(version => new { version.PluginId, version.VersionNumber })
                .IsUnique();

            // RN13: no máximo uma versão atual por plugin.
            entity.HasIndex(version => version.PluginId)
                .IsUnique()
                .HasFilter("[IsCurrent] = 1")
                .HasDatabaseName("IX_PluginVersions_PluginId_Current");

            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_PluginVersions_Status", "[Status] IN (1, 3)");
                // RNF04: arquivo de até 100 MB.
                table.HasCheckConstraint(
                    "CK_PluginVersions_FileSize",
                    "[FileSize] > 0 AND [FileSize] <= 104857600");
            });
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

            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Orders_Status", "[Status] IN (1, 2, 3, 4, 5)");
                table.HasCheckConstraint("CK_Orders_TotalAmount", "[TotalAmount] > 0");
            });
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

            entity.HasIndex(item => new { item.OrderId, item.PluginId })
                .IsUnique();
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

            // Índice único em coluna anulável: o EF gera o filtro IS NOT NULL.
            entity.HasIndex(payment => payment.GatewayPaymentId)
                .IsUnique();

            entity.Property(payment => payment.CheckoutUrl)
                .HasMaxLength(1000);

            entity.Property(payment => payment.IdempotencyKey)
                .IsRequired();

            entity.HasIndex(payment => payment.IdempotencyKey)
                .IsUnique();

            entity.Property(payment => payment.Status)
                .IsRequired();

            entity.Property(payment => payment.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(payment => payment.Method)
                .HasMaxLength(30);

            entity.Property(payment => payment.CreatedAt)
                .IsRequired();

            entity.HasOne(payment => payment.Order)
                .WithOne(order => order.Payment)
                .HasForeignKey<Payment>(payment => payment.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable(table =>
                table.HasCheckConstraint("CK_Payments_Status", "[Status] IN (1, 2, 3, 4, 5)"));
        });
    }

    private static void ConfigureCustomerLibraries(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerLibrary>(entity =>
        {
            entity.HasKey(library => library.Id);

            entity.Property(library => library.AccessType)
                .IsRequired();

            entity.Property(library => library.Status)
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

            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_CustomerLibraries_AccessType", "[AccessType] IN (1, 2, 3)");
                table.HasCheckConstraint("CK_CustomerLibraries_Status", "[Status] IN (1, 2)");
            });
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

    private static void ConfigureWebhookEvents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebhookEvent>(entity =>
        {
            entity.HasKey(webhook => webhook.Id);

            entity.Property(webhook => webhook.Gateway)
                .IsRequired()
                .HasMaxLength(80);

            // RN08: a mesma notificação (x-request-id) só é registrada uma vez.
            entity.Property(webhook => webhook.ExternalEventId)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(webhook => webhook.ExternalEventId)
                .IsUnique();

            entity.Property(webhook => webhook.Topic)
                .IsRequired()
                .HasMaxLength(60);

            entity.Property(webhook => webhook.ResourceId)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(webhook => webhook.Payload)
                .IsRequired();

            entity.Property(webhook => webhook.Attempts)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(webhook => webhook.ReceivedAt)
                .IsRequired();

            entity.HasOne(webhook => webhook.Payment)
                .WithMany(payment => payment.WebhookEvents)
                .HasForeignKey(webhook => webhook.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAuditLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(audit => audit.Id);

            entity.Property(audit => audit.Id)
                .UseIdentityColumn();

            entity.Property(audit => audit.Action)
                .IsRequired()
                .HasMaxLength(60);

            entity.Property(audit => audit.Resource)
                .IsRequired()
                .HasMaxLength(40);

            entity.Property(audit => audit.ResourceId)
                .HasMaxLength(64);

            entity.Property(audit => audit.IpAddress)
                .HasMaxLength(80);

            entity.Property(audit => audit.CreatedAt)
                .IsRequired();

            // Auditoria sobrevive ao usuário: a referência vira nula.
            entity.HasOne(audit => audit.User)
                .WithMany()
                .HasForeignKey(audit => audit.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
