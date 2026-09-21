using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VehicleProfitTracker.API.Data;

#nullable disable

namespace VehicleProfitTracker.API.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260920120000_AddAdminAnalyticsAndSubscriptions")]
public partial class AddAdminAnalyticsAndSubscriptions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Role", table: "users", type: "character varying(20)", maxLength: 20,
            nullable: false, defaultValue: "User");

        migrationBuilder.CreateTable(
            name: "user_events",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Platform = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                AppVersion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_events", x => x.Id);
                table.ForeignKey("FK_user_events_users_UserId", x => x.UserId, "users", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "subscriptions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Store = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                ProductId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                AutoRenewEnabled = table.Column<bool>(type: "boolean", nullable: false),
                ExternalPurchaseId = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_subscriptions", x => x.Id);
                table.ForeignKey("FK_subscriptions_users_UserId", x => x.UserId, "users", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_user_events_OccurredAt_Name", table: "user_events", columns: new[] { "OccurredAt", "Name" });
        migrationBuilder.CreateIndex(name: "IX_user_events_UserId_OccurredAt", table: "user_events", columns: new[] { "UserId", "OccurredAt" });
        migrationBuilder.CreateIndex(name: "IX_subscriptions_UserId_Status", table: "subscriptions", columns: new[] { "UserId", "Status" });
        migrationBuilder.CreateIndex(name: "IX_subscriptions_Store_ExternalPurchaseId", table: "subscriptions", columns: new[] { "Store", "ExternalPurchaseId" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "subscriptions");
        migrationBuilder.DropTable(name: "user_events");
        migrationBuilder.DropColumn(name: "Role", table: "users");
    }
}
