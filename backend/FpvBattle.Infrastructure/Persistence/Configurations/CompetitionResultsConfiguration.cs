using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class CompetitionResultsConfiguration : IEntityTypeConfiguration<CompetitionResults>
{
    public void Configure(EntityTypeBuilder<CompetitionResults> builder)
    {
        builder.ToTable("CompetitionResults");
        builder.Property(c => c.ModelName).HasMaxLength(128);
        builder.Ignore(c => c.GapToLeaderPercent);
    }
}
