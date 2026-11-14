using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class DayStreakFreezeConfiguration : IEntityTypeConfiguration<DayStreakFreeze>
{
    public void Configure(EntityTypeBuilder<DayStreakFreeze> builder)
    {
        builder.ToTable("DayStreakFreezes");
    }
}
