using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class PilotLeagueConfiguration : IEntityTypeConfiguration<PilotLeague>
{
    public void Configure(EntityTypeBuilder<PilotLeague> builder)
    {
        builder.ToTable("PilotLeagues");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.CupId).HasMaxLength(64).IsRequired();
        builder.Property(p => p.League).HasMaxLength(64);
        builder.HasIndex(p => new { p.PilotId, p.CupId });
    }
}
