using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleProfitTracker.API.Migrations;

[Migration("20260924070000_AddAppSettings")]
public partial class AddAppSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "app_settings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PasswordResetEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                SubscriptionsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_app_settings", x => x.Id);
            });

        migrationBuilder.InsertData(
            table: "app_settings",
            columns: new[] { "Id", "PasswordResetEnabled", "SubscriptionsEnabled", "CreatedAt", "UpdatedAt" },
            values: new object[] { Guid.Parse("00000000-0000-0000-0000-000000000001"), true, true, DateTime.UtcNow, DateTime.UtcNow });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "app_settings");
    }
}