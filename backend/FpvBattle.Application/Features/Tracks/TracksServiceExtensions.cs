using FpvBattle.Application.Features.Tracks.Jobs;
using FpvBattle.Application.Features.Tracks.Services;
using FpvBattle.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Application.Features.Tracks;

public static class TracksServiceExtensions
{
    public static IServiceCollection AddTracks(this IServiceCollection services)
    {
        services.AddScoped<TrackLookupSync>();
        services.AddScoped<SyncTrackLookupsJob>();
        services.AddScoped<IJobRegistrar, TracksJobRegistrar>();
        return services;
    }
}
