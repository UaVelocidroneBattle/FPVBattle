using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class TeamMembershipRequestConfiguration : IEntityTypeConfiguration<TeamMembershipRequest>
{
    public void Configure(EntityTypeBuilder<TeamMembershipRequest> builder)
    {
        builder.ToTable("TeamMembershipRequests");
        builder.HasKey(i => new { i.TeamId, i.UserId });
        builder.HasIndex(i => i.UserId);
        builder.HasQueryFilter(i => !i.Team.IsDeleted);
        builder.HasOne(i => i.Team)
            .WithMany(t => t.MembershipRequests)
            .HasForeignKey(i => i.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.InvitedByUser)
            .WithMany()
            .HasForeignKey(i => i.InvitedByUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
