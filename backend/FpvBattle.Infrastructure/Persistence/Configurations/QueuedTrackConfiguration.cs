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
        builder.HasOne(q => q.ScheduledBy)
            .WithMany()
            .HasForeignKey(q => q.ScheduledByUserId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(q => new { q.ScheduledByUserId, q.AddedOn });
        builder.HasIndex(q => q.TrackId).IsUnique().HasFilter("\"Used\" = 0");
    }
}
