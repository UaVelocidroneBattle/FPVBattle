using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class QueuedTrackConfiguration : IEntityTypeConfiguration<QueuedTrack>
{
    public void Configure(EntityTypeBuilder<QueuedTrack> builder)
    {
        builder.ToTable("TrackQueue");
        builder.HasKey(p => p.Id);
    }
}
