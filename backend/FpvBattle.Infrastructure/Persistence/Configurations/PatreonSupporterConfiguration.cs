using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class PatreonSupporterConfiguration : IEntityTypeConfiguration<PatreonSupporter>
{
    public void Configure(EntityTypeBuilder<PatreonSupporter> builder)
    {
        builder.ToTable("PatreonSupporters");
        builder.HasKey(p => p.PatreonId);
        builder.Property(p => p.PatreonId).HasMaxLength(128);
        builder.Property(p => p.Name).HasMaxLength(256);
        builder.Property(p => p.Email).HasMaxLength(256);
        builder.Property(p => p.TierName).HasMaxLength(128);
        builder.Property(p => p.Status).HasMaxLength(64);
    }
}
