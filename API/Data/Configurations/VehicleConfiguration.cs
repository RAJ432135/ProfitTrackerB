using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleProfitTracker.API.Models;

namespace VehicleProfitTracker.API.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.VehicleNumber).IsRequired().HasMaxLength(20);
        builder.Property(v => v.VehicleType).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(v => new { v.UserId, v.VehicleNumber });

        builder.HasMany(v => v.Transactions)
            .WithOne(t => t.Vehicle)
            .HasForeignKey(t => t.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
