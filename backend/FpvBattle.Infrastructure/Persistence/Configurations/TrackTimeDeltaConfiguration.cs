using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class TrackTimeDeltaConfiguration : IEntityTypeConfiguration<TrackTimeDelta>
{
    public void Configure(EntityTypeBuilder<TrackTimeDelta> builder)
    {
        builder.ToTable("TrackTimeDeltas");
        builder.Property(t => t.ModelName).HasMaxLength(128);
        builder.Property(t => t.Country).HasMaxLength(8).HasDefaultValue("UA");
    }
}
