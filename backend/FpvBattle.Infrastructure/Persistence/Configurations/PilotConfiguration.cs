using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class PilotConfiguration : IEntityTypeConfiguration<Pilot>
{
    public void Configure(EntityTypeBuilder<Pilot> builder)
    {
        builder.ToTable("Pilots");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Name).HasMaxLength(128);
        builder.Property(p => p.Country).HasMaxLength(8).HasDefaultValue("UA");
    }
}
