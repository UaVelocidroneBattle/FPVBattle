using Veloci.Web.Infrastructure.Hangfire;

namespace Veloci.Web.Infrastructure;

public class DefaultInit
{
    public static async Task InitializeAsync(IConfiguration configuration, WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        HangfireInit.InitRecurrentJobs(configuration, app.Services);
    }
}