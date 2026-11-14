using FpvBattle.Web.Infrastructure.Hangfire;

namespace FpvBattle.Web.Infrastructure;

public class DefaultInit
{
    public static async Task InitializeAsync(IConfiguration configuration, WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        HangfireInit.InitRecurrentJobs(configuration, app.Services);
    }
}