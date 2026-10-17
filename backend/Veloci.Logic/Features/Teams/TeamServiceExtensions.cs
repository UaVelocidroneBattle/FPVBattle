using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Veloci.Logic.Features.Teams.Services;

namespace Veloci.Logic.Features.Teams;

public static class TeamServiceExtensions
{
    public static IServiceCollection AddTeams(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TeamSettings>(configuration.GetSection(TeamSettings.SectionName));
        services.AddScoped<TeamService>();
        services.AddScoped<TeamCreationPolicy>();
        services.AddScoped<TeamTagLookup>();
        return services;
    }
}
