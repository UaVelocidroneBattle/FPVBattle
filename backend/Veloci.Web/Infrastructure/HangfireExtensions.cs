using Hangfire;
using Hangfire.Storage.SQLite;
using SQLite;
using Veloci.Web.Infrastructure.Hangfire;

namespace Veloci.Web.Infrastructure;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var hangfireDbPath = configuration.GetConnectionString("HangfireDbPath") ??
                             throw new InvalidOperationException("'HangfireDbPath' not found.");

        //sometime hangfire db gets corrupted, it looks like this is the fix:
        //https://github.com/raisedapp/Hangfire.Storage.SQLite/issues/79

        services.AddHangfire(config => config
            .UseSQLiteStorage(new SQLiteDbConnectionFactory(() => new SQLiteConnection(
                    databasePath: hangfireDbPath,
                    openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex,
                    storeDateTimeAsTicks: true
                )
                {
                    BusyTimeout = TimeSpan.FromSeconds(value: 10)
                }))
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
        );

        // Add global job execution logging filter
        GlobalJobFilters.Filters.Add(new JobExecutionLoggingAttribute());

        services.AddHangfireServer(o =>
        {
            o.WorkerCount = 1;
        });

        return services;
    }
}
