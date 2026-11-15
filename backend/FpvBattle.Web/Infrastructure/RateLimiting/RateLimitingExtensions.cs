using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace FpvBattle.Web.Infrastructure.RateLimiting;

// Partitions by client IP, which comes from X-Forwarded-For: UseRateLimiter must run after UseForwardedHeaders.
public static class RateLimitingExtensions
{
    private static readonly PathString ApiPath = "/api";
    private static readonly PathString AuthPath = "/api/auth";
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
                      ?? new RateLimitingOptions();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.OnRejected = WriteRetryAfterHeader;
            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                context => SelectPartition(context, options));
        });

        return services;
    }

    private static RateLimitPartition<string> SelectPartition(HttpContext context, RateLimitingOptions options)
    {
        var path = context.Request.Path;

        if (!options.Enabled || !path.StartsWithSegments(ApiPath))
            return RateLimitPartition.GetNoLimiter(string.Empty);

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return path.StartsWithSegments(AuthPath)
            ? PerMinute($"auth:{clientIp}", options.AuthRequestsPerMinute)
            : PerMinute($"api:{clientIp}", options.ApiRequestsPerMinute);
    }

    private static RateLimitPartition<string> PerMinute(string key, int permits) =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permits,
            Window = Window,
            QueueLimit = 0
        });

    private static ValueTask WriteRetryAfterHeader(OnRejectedContext context, CancellationToken _)
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        return ValueTask.CompletedTask;
    }
}
