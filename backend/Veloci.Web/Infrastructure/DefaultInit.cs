using Veloci.Logic.Services;
using Veloci.Logic.Services.Leagues;
using Veloci.Web.Infrastructure.Hangfire;

namespace Veloci.Web.Infrastructure;

public class DefaultInit
{
    public static async Task InitializeAsync(IConfiguration configuration, WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var leagueService = scope.ServiceProvider.GetService<LeagueService>();
        await leagueService.UpdateLeaguesAsync();

        var leagueQualifier = scope.ServiceProvider.GetService<LeagueQualifier>();
        await leagueQualifier.QualifyPilotsAsync();

        HangfireInit.InitRecurrentJobs(configuration, app.Services);
    }
}
