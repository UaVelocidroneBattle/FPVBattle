using System.Net;
using Hangfire;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
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
using FpvBattle.Infrastructure.Auth;
using FpvBattle.Infrastructure.Discord;
using FpvBattle.Infrastructure.Persistence;
using FpvBattle.Infrastructure.Telegram;
using FpvBattle.Domain;
using FpvBattle.Hangfire.Metrics;
using FpvBattle.Application.Bot;
using FpvBattle.Application.Bot.Telegram.Commands.Core;
using FpvBattle.Application.Features.Auth;
using FpvBattle.Application.Features.PilotBinding;
using FpvBattle.Application.Features.Notifications;
using FpvBattle.Application.Notifications;
using ModelContextProtocol.AspNetCore;
using FpvBattle.Application.Features.Teams;
using FpvBattle.Application.Services;
using FpvBattle.Mcp.Tools;
using FpvBattle.Web.Infrastructure.Hangfire;
using FpvBattle.Web.Infrastructure.Logging;
using FpvBattle.Web.Infrastructure.Mcp;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace FpvBattle.Web.Infrastructure;

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

        services.AddMemoryCache();

        services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddAuthFeature(Configuration);
        services.AddTokenAuthentication(Configuration);
        services.AddPilotBinding();
        services.AddNotifications();
        services.AddTeams(Configuration);

        services.AddControllersWithViews();

        services.Configure<LoggerConfig>(Configuration.GetSection("Logger"));
        services.Configure<ResultsOptions>(Configuration.GetSection(ResultsOptions.SectionName));

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
            .AddBotMessaging()
            .AddTelegramBot()
            .AddDiscordBot();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IntermediateCompetitionResult>());

        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer((schema, context, cancellationToken) =>
            {
                if (schema.Type.HasValue && schema.Type.Value.HasFlag(JsonSchemaType.Integer))
                {
                    schema.Type = JsonSchemaType.Integer;
                    schema.Pattern = null;
                }
                return Task.CompletedTask;
            });
        });

        services
            .AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly(typeof(PilotsMcpTools).Assembly);
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

        var mcpApiKey = app.Configuration["MCP:Key"];
        app.ProtectMcpEndpoints("/mcp", mcpApiKey);
        app.MapMcp("/mcp");

        // Skip HTTPS redirection for MCP endpoint (MCP client may not handle redirects)
        app.UseWhen(
            context => !context.Request.Path.StartsWithSegments("/mcp"),
            appBuilder => appBuilder.UseHttpsRedirection());

        app.UseStaticFiles();

        var user = app.Configuration["PrometheusAuth:Username"];
        var pass = app.Configuration["PrometheusAuth:Password"];

        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
        {
            app.ProtectUrl("/metrics", "Prometheus", user, pass);
        }

        app.UseRouting();

        app.UseAuthentication();
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
