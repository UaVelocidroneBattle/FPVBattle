using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class QuadModelConfiguration : IEntityTypeConfiguration<QuadModel>
{
    public void Configure(EntityTypeBuilder<QuadModel> builder)
    {
        builder.ToTable("QuadModels");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Name).HasMaxLength(128);
    }
}
