using System.Net;
using Hangfire;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using Veloci.Data;
using Veloci.Hangfire.Metrics;
using Veloci.Logic.API.Options;
using Veloci.Logic.Bot;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Bot.Telegram.Commands.Core;
using Veloci.Logic.Notifications;
using Veloci.Web.Infrastructure.Hangfire;
using Veloci.Web.Infrastructure.Logging;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

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

        var otel = builder.Services.AddOpenTelemetry();
        otel.ConfigureResource(resource => resource
            .AddService(serviceName: builder.Environment.ApplicationName));

        otel.WithMetrics(metrics => metrics
            // Metrics provider from OpenTelemetry
            .AddAspNetCoreInstrumentation()
            .AddProcessInstrumentation()
            .AddRuntimeInstrumentation()
            .AddSqlClientInstrumentation()
            .AddHangfireInstrumentation()
            // Metrics provides by ASP.NET Core in .NET 8
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
            // Metrics provided by System.Net libraries
            .AddMeter("System.Net.Http")
            .AddMeter("System.Net.NameResolution")
            .AddPrometheusExporter());

        var OtlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (OtlpEndpoint != null)
        {
            otel.UseOtlpExporter();
        }
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

        services
            .AddControllersWithViews()
            .ConfigureApplicationPartManager(apm =>
            {
                var assembly = typeof(Veloci.Logic.Features.Patreon.PatreonController).Assembly;
                if (!apm.ApplicationParts.Any(part => part is AssemblyPart assemblyPart && assemblyPart.Assembly == assembly))
                {
                    apm.ApplicationParts.Add(new AssemblyPart(assembly));
                }
            });

        // Configure view location formats to support Features folder structure in RCL
        services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
        {
            // Add our Features folder patterns to view discovery
            // {0} = action name, {1} = controller name
            options.ViewLocationFormats.Add("/Features/{1}/Views/{1}/{0}.cshtml");
            options.ViewLocationFormats.Add("/Features/{1}/Views/Shared/{0}.cshtml");
        });

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

        services
            .AddHangfireConfiguration(Configuration)
            .RegisterCustomServices(Configuration)
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
        if (Configuration.GetValue("RunMigrations", false))
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
        }

        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();

        app.UseCors("AllowAnyOrigin");

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            KnownNetworks = { new IPNetwork(IPAddress.Parse("172.17.0.0"), 16) }
        });

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        var user = app.Configuration["PrometheusAuth:Username"];
        var pass = app.Configuration["PrometheusAuth:Password"];

        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
        {
            app.ProtectUrl("/metrics", "Prometheus", user, pass);
        }

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();
        app.MapHangfireDashboard(new DashboardOptions
        {
            Authorization = [new HangfireAuthorizationFilter()],
        });

        app.MapPrometheusScrapingEndpoint();
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

            if (!string.IsNullOrEmpty(logconfig?.Value.SeqToken))
            {
                var token = logconfig?.Value.SeqToken;
                logger.WriteTo.Seq(logconfig.Value.SeqUrl, LogEventLevel.Verbose, 1000, null, apiKey:token);
            }

            logger.WriteTo.Console(outputTemplate: LoggingConstants.ConsoleOutputTemplate);
        });
    }
}
