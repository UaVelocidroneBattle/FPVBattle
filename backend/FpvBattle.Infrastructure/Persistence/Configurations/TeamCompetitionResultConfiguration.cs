using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class TeamCompetitionResultConfiguration : IEntityTypeConfiguration<TeamCompetitionResult>
{
    public void Configure(EntityTypeBuilder<TeamCompetitionResult> builder)
    {
        builder.ToTable("TeamCompetitionResults");
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.CompetitionId, r.TeamId }).IsUnique();
        builder.HasIndex(r => r.TeamId);
        builder.HasQueryFilter(r => !r.Team.IsDeleted);
        builder.HasOne(r => r.Competition)
            .WithMany(c => c.TeamCompetitionResults)
            .HasForeignKey(r => r.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.Team)
            .WithMany()
            .HasForeignKey(r => r.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
