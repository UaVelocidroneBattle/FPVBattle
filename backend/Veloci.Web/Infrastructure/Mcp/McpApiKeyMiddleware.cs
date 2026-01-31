namespace Veloci.Web.Infrastructure.Mcp;

public static class McpApiKeyMiddleware
{
    private const string ApiKeyHeaderName = "X-MCP-API-Key";

    public static void ProtectMcpEndpoints(this WebApplication app, string path, string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return;
        }

        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments(path))
            {
                var providedKey = context.Request.Headers[ApiKeyHeaderName].FirstOrDefault();

                if (string.IsNullOrEmpty(providedKey) || providedKey != apiKey)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized: Invalid or missing API key");
                    return;
                }
            }

            await next();
        });
    }
}