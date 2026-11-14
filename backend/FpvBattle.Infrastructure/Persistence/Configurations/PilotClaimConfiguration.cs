using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class PilotClaimConfiguration : IEntityTypeConfiguration<PilotClaim>
{
    public void Configure(EntityTypeBuilder<PilotClaim> builder)
    {
        builder.ToTable("PilotClaims");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.PilotName).HasMaxLength(128).IsRequired();
        builder.HasIndex(c => c.UserId).IsUnique();
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
