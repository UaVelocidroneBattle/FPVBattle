using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> builder)
    {
        builder.ToTable("Competitions");
        builder.Property(c => c.CupId).HasMaxLength(64).IsRequired().HasDefaultValue("open-class");
        builder.HasIndex(c => c.StartedOn);
    }
}
