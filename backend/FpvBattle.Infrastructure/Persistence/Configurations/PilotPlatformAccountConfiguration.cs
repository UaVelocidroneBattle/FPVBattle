using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class PilotPlatformAccountConfiguration : IEntityTypeConfiguration<PilotPlatformAccount>
{
    public void Configure(EntityTypeBuilder<PilotPlatformAccount> builder)
    {
        builder.ToTable("PilotPlatformAccounts");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Username).HasMaxLength(128);
    }
}
