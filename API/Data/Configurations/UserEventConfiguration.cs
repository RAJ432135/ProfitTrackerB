using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleProfitTracker.API.Models;

namespace VehicleProfitTracker.API.Data.Configurations;

public class UserEventConfiguration : IEntityTypeConfiguration<UserEvent>
{
    public void Configure(EntityTypeBuilder<UserEvent> builder)
    {
        builder.ToTable("user_events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(80);
        builder.Property(e => e.Platform).HasMaxLength(30);
        builder.Property(e => e.AppVersion).HasMaxLength(30);
        builder.HasIndex(e => new { e.OccurredAt, e.Name });
        builder.HasIndex(e => new { e.UserId, e.OccurredAt });
    }
}
