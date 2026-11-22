using FpvBattle.Application.Features.Tracks.Jobs;
using FpvBattle.Application.Features.Tracks.Lookups;
using FpvBattle.Application.Features.Tracks.Queue;
using FpvBattle.Application.Features.Tracks.Scheduling;
using FpvBattle.Application.Features.Tracks.Selection;
using FpvBattle.Application.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Application.Features.Tracks;

public static class TracksServiceExtensions
{
    public static IServiceCollection AddTracks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<TrackLookupSync>();
        services.AddScoped<SyncTrackLookupsJob>();
        services.AddScoped<IJobRegistrar, TracksJobRegistrar>();

        services.AddScoped<TrackService>();
        services.AddScoped<TrackQueueService>();

        services.Configure<TrackSchedulingOptions>(configuration.GetSection(TrackSchedulingOptions.SectionName));
        services.AddScoped<PatronQuotaPolicy>();
        services.AddScoped<TrackSchedulingService>();

        return services;
    }
}
