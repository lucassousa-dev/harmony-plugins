using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarmonyPlugins.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlinhamentoModeloTG : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PluginVersions_PluginId",
                table: "PluginVersions");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1); // Usuários já cadastrados ficam ATIVOS (o CHECK não aceita 0).

            migrationBuilder.AddColumn<string>(
                name: "Compatibility",
                table: "PluginVersions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileSha256",
                table: "PluginVersions",
                type: "char(64)",
                unicode: false,
                fixedLength: true,
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "GameId",
                table: "Plugins",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "Plugins",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdempotencyKey",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "Payments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CustomerLibraries",
                type: "int",
                nullable: false,
                defaultValue: 1); // Aquisições existentes ficam ATIVAS (o CHECK não aceita 0).

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CustomerLibraries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ResourceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Gateway = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ExternalEventId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ResourceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookEvents_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Role",
                table: "Users",
                sql: "[Role] IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Status",
                table: "Users",
                sql: "[Status] IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_PluginVersions_PluginId_Current",
                table: "PluginVersions",
                column: "PluginId",
                unique: true,
                filter: "[IsCurrent] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PluginVersions_PluginId_VersionNumber",
                table: "PluginVersions",
                columns: new[] { "PluginId", "VersionNumber" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PluginVersions_FileSize",
                table: "PluginVersions",
                sql: "[FileSize] > 0 AND [FileSize] <= 104857600");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PluginVersions_Status",
                table: "PluginVersions",
                sql: "[Status] IN (1, 3)");

            migrationBuilder.CreateIndex(
                name: "IX_Plugins_GameId",
                table: "Plugins",
                column: "GameId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Plugins_Price",
                table: "Plugins",
                sql: "([Type] = 1 AND [Price] = 0) OR ([Type] = 2 AND [Price] > 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Plugins_Status",
                table: "Plugins",
                sql: "[Status] IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Plugins_Type",
                table: "Plugins",
                sql: "[Type] IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GatewayPaymentId",
                table: "Payments",
                column: "GatewayPaymentId",
                unique: true,
                filter: "[GatewayPaymentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_IdempotencyKey",
                table: "Payments",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Status",
                table: "Payments",
                sql: "[Status] IN (1, 2, 3, 4, 5)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_Status",
                table: "Orders",
                sql: "[Status] IN (1, 2, 3, 4, 5)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_TotalAmount",
                table: "Orders",
                sql: "[TotalAmount] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId_PluginId",
                table: "OrderItems",
                columns: new[] { "OrderId", "PluginId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CustomerLibraries_AccessType",
                table: "CustomerLibraries",
                sql: "[AccessType] IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CustomerLibraries_Status",
                table: "CustomerLibraries",
                sql: "[Status] IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Name",
                table: "Games",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEvents_ExternalEventId",
                table: "WebhookEvents",
                column: "ExternalEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEvents_PaymentId",
                table: "WebhookEvents",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plugins_Games_GameId",
                table: "Plugins",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plugins_Games_GameId",
                table: "Plugins");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "WebhookEvents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Role",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Status",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_PluginVersions_PluginId_Current",
                table: "PluginVersions");

            migrationBuilder.DropIndex(
                name: "IX_PluginVersions_PluginId_VersionNumber",
                table: "PluginVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PluginVersions_FileSize",
                table: "PluginVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PluginVersions_Status",
                table: "PluginVersions");

            migrationBuilder.DropIndex(
                name: "IX_Plugins_GameId",
                table: "Plugins");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Plugins_Price",
                table: "Plugins");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Plugins_Status",
                table: "Plugins");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Plugins_Type",
                table: "Plugins");

            migrationBuilder.DropIndex(
                name: "IX_Payments_GatewayPaymentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_IdempotencyKey",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Status",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_Status",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_TotalAmount",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId_PluginId",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CustomerLibraries_AccessType",
                table: "CustomerLibraries");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CustomerLibraries_Status",
                table: "CustomerLibraries");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Compatibility",
                table: "PluginVersions");

            migrationBuilder.DropColumn(
                name: "FileSha256",
                table: "PluginVersions");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "Plugins");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "Plugins");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CustomerLibraries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CustomerLibraries");

            migrationBuilder.CreateIndex(
                name: "IX_PluginVersions_PluginId",
                table: "PluginVersions",
                column: "PluginId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");
        }
    }
}
