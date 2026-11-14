using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class PilotAchievementConfiguration : IEntityTypeConfiguration<PilotAchievement>
{
    public void Configure(EntityTypeBuilder<PilotAchievement> builder)
    {
        builder.ToTable("PilotAchievements");
    }
}
