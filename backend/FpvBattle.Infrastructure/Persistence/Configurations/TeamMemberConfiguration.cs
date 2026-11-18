using FpvBattle.Application.Features.Teams;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("TeamMembers");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => m.UserId).IsUnique().HasFilter("\"LeftOn\" IS NULL");
        builder.HasQueryFilter("TeamNotDeleted", m => !m.Team.IsDeleted);
        builder.HasQueryFilter(TeamMemberQueries.CurrentMembersFilter, m => m.LeftOn == null);
        builder.HasOne(m => m.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
