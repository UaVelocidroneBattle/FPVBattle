using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class TrackTimeConfiguration : IEntityTypeConfiguration<TrackTime>
{
    public void Configure(EntityTypeBuilder<TrackTime> builder)
    {
        builder.ToTable("TrackTimes");
        builder.HasIndex(t => t.UserId);
        builder.Property(t => t.PlayerName).HasMaxLength(128);
        builder.Property(t => t.ModelName).HasMaxLength(128);
        builder.Property(t => t.Country).HasMaxLength(8).HasDefaultValue("UA");
    }
}
