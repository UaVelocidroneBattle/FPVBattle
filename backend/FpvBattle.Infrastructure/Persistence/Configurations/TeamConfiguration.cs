using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(Team.NameMaxLength).IsRequired().UseCollation("NOCASE");
        builder.Property(t => t.Description).HasMaxLength(Team.DescriptionMaxLength);
        builder.HasIndex(t => t.Name).IsUnique().HasFilter("NOT \"IsDeleted\"");
        builder.Property(t => t.Tag).HasMaxLength(Team.TagMaxLength).UseCollation("NOCASE");
        builder.HasIndex(t => t.Tag).IsUnique().HasFilter("NOT \"IsDeleted\"");
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
