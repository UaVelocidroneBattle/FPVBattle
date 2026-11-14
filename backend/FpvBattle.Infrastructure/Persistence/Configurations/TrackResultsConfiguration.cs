using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class TrackResultsConfiguration : IEntityTypeConfiguration<TrackResults>
{
    public void Configure(EntityTypeBuilder<TrackResults> builder)
    {
        builder.ToTable("TrackResults");
    }
}
