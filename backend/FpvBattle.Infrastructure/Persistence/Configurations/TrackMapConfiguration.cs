using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class TrackMapConfiguration : IEntityTypeConfiguration<TrackMap>
{
    public void Configure(EntityTypeBuilder<TrackMap> builder)
    {
        builder.ToTable("TrackMaps");
    }
}
