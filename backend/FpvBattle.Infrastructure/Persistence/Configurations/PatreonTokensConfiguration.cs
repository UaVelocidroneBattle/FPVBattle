using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class PatreonTokensConfiguration : IEntityTypeConfiguration<PatreonTokens>
{
    public void Configure(EntityTypeBuilder<PatreonTokens> builder)
    {
        builder.ToTable("PatreonTokens");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.AccessToken).HasMaxLength(2048);
        builder.Property(p => p.RefreshToken).HasMaxLength(2048);
        builder.Property(p => p.Scope).HasMaxLength(256);
    }
}
