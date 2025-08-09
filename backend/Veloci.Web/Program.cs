using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Veloci.Web.Infrastructure;
using Veloci.Web.Infrastructure.Logging;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .WriteTo.Console(outputTemplate: LoggingConstants.ConsoleOutputTemplate)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
var startup = new Startup(builder.Configuration);

startup.ConfigureBuilder(builder);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app);

try
{
    await DefaultInit.InitializeAsync(builder.Configuration, app);
    Log.Information("Application started.");
    app.Run();
}
catch (Exception ex)
{
    Log.Error(ex, "Failed to start application");
}
