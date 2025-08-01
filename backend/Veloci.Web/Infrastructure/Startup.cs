using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using Veloci.Data;
using Veloci.Logic.Achievements.Base;
using Veloci.Logic.API.Options;
using Veloci.Logic.Bot;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Bot.Telegram.Commands.Core;
using Veloci.Logic.Notifications;
using Veloci.Web.Infrastructure.Hangfire;

namespace Veloci.Web.Infrastructure;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureBuilder(WebApplicationBuilder builder)
    {
        ConfigureLogging(builder);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        var connectionString = Configuration.GetConnectionString("DefaultConnection") ??
                               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseLazyLoadingProxies()
                .UseSqlite(connectionString));

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddControllersWithViews();

        services.Configure<LoggerConfig>(Configuration.GetSection("Logger"));
        services.Configure<ApiSettings>(Configuration.GetSection("API"));

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireDigit = false;
            options.Password.RequiredUniqueChars = 0;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAnyOrigin", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddHangfire(config => config
            .UseSQLiteStorage(new SqliteConnectionStringBuilder(connectionString).DataSource)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
        );

        // Add global job execution logging filter
        GlobalJobFilters.Filters.Add(new JobExecutionLoggingAttribute());

        services.AddHangfireServer(o =>
        {
            o.WorkerCount = 1;
        });

        services
            .RegisterCustomServices(Configuration)
            .RegisterAchievements()
            .RegisterTelegramCommands()
            .UseTelegramBotService()
            .UseDiscordBotService();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IntermediateCompetitionResult>());

        services.AddOpenApi();
    }

    public void Configure(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
            app.MapOpenApi();
        }
        else if (app.Environment.IsProduction())
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
            }

            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseCors("AllowAnyOrigin");

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();
        app.MapHangfireDashboard(new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() },
        });
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, provider, configuration) =>
        {
            var logconfig = provider.GetService<IOptions<LoggerConfig>>();

            var logger = configuration
                .MinimumLevel.Debug()

                .MinimumLevel.Override("Hangfire.Processing.BackgroundExecution", LogEventLevel.Warning)
                .MinimumLevel.Override("Hangfire.Storage.SQLite.ExpirationManager", LogEventLevel.Warning)
                .MinimumLevel.Override("Hangfire.Storage.SQLite.CountersAggregator", LogEventLevel.Warning)
                .MinimumLevel.Override("Hangfire.Server.ServerHeartbeatProcess", LogEventLevel.Warning)
                .MinimumLevel.Override("Hangfire.Server.RecurringJobScheduler", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails();

            if (!builder.Environment.IsDevelopment())
            {
                logger.MinimumLevel.Override("Microsoft.AspNetCore.ResponseCaching.ResponseCachingMiddleware", LogEventLevel.Warning);
            }

            if (logconfig?.Value?.Path != null)
            {
                logger.WriteTo.File(
                    Path.Join(logconfig.Value.Path, "log.log"),
                    rollingInterval: RollingInterval.Day,
                    buffered: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(10));
            }

            if (!string.IsNullOrEmpty(logconfig?.Value.SematextToken))
            {
                var token = logconfig?.Value.SematextToken;
                logger.WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(new Uri($@"https://logsene-receiver.eu.sematext.com/{token}/_doc/"))
                    {
                        AutoRegisterTemplate = true,
                        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                        CustomFormatter = new ElasticsearchJsonFormatter()
                    });
            }

            logger.WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
        });
    }
}
