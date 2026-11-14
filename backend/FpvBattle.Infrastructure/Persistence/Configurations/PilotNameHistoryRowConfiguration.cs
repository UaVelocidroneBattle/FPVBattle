using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class PilotNameHistoryRowConfiguration : IEntityTypeConfiguration<PilotNameHistoryRow>
{
    public void Configure(EntityTypeBuilder<PilotNameHistoryRow> builder)
    {
        builder.ToTable("PilotNameHistory");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.OldName).HasMaxLength(128);
        builder.Property(p => p.NewName).HasMaxLength(128);
    }
}
