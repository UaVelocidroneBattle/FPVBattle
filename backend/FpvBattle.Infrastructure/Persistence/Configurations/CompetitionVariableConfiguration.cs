using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpvBattle.Infrastructure.Persistence.Configurations;

public sealed class CompetitionVariableConfiguration : IEntityTypeConfiguration<CompetitionVariable>
{
    public void Configure(EntityTypeBuilder<CompetitionVariable> builder)
    {
        builder.ToTable("CompetitionVariables");
    }
}
