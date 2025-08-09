namespace Veloci.Web.Infrastructure.Logging;

public static class LoggingConstants
{
    public const string ConsoleOutputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
}