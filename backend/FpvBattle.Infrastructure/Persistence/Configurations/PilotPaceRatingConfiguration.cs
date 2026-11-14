using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class PilotPaceRatingConfiguration : IEntityTypeConfiguration<PilotPaceRating>
{
    public void Configure(EntityTypeBuilder<PilotPaceRating> builder)
    {
        builder.ToTable("PilotPaceRatings");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.CupId).HasMaxLength(64).IsRequired();
        builder.HasIndex(p => new { p.PilotId, p.CupId });
    }
}
