namespace FpvBattle.Web.Infrastructure.RateLimiting;

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public bool Enabled { get; set; } = true;
    public int ApiRequestsPerMinute { get; set; } = 300;
    public int AuthRequestsPerMinute { get; set; } = 20;
}
