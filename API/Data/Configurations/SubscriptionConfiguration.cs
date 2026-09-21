using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleProfitTracker.API.Models;

namespace VehicleProfitTracker.API.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Store).IsRequired().HasMaxLength(30);
        builder.Property(s => s.ProductId).IsRequired().HasMaxLength(120);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.ExternalPurchaseId).HasMaxLength(300);
        builder.HasIndex(s => new { s.UserId, s.Status });
        builder.HasIndex(s => new { s.Store, s.ExternalPurchaseId }).IsUnique();
    }
}
