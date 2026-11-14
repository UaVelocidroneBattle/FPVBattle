using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(256);
        builder.Property(u => u.Locale).HasMaxLength(16);
        builder.HasIndex(u => u.PilotId).IsUnique();
        builder.HasOne(u => u.Pilot)
            .WithMany()
            .HasForeignKey(u => u.PilotId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
