using FpvBattle.Application.Features.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(new IdentityRole
        {
            Id = "6f1c2b8e-3d4a-4e5f-9a7b-1c2d3e4f5a6b",
            Name = Roles.Conductor,
            NormalizedName = Roles.Conductor.ToUpperInvariant(),
            ConcurrencyStamp = "b2d6e0f4-8a1c-4e3b-9d5f-7a2c4e6b8d0f"
        });
    }
}
