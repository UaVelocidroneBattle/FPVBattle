using FpvBattle.Application.Features.Teams.Leaderboard;
using FpvBattle.Application.Features.Teams.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Application.Features.Teams;

public static class TeamServiceExtensions
{
    public static IServiceCollection AddTeams(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TeamSettings>(configuration.GetSection(TeamSettings.SectionName));
        services.AddScoped<TeamService>();
        services.AddScoped<TeamCreationPolicy>();
        services.AddScoped<TeamTagLookup>();
        services.AddScoped<TeamRosterLookup>();
        services.AddScoped<TeamLeaderboardCalculator>();
        services.AddScoped<TeamLeaderboardService>();
        return services;
    }
}
