namespace Veloci.Web.Infrastructure;

public static class UrlProtector
{
    public static void ProtectUrl(this WebApplication app, string path, string realm, string login, string password)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments(path))
            {
                string authHeader = context.Request.Headers["Authorization"];
                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                    var encoded = authHeader.Substring("Basic ".Length).Trim();
                    var credentialBytes = Convert.FromBase64String(encoded);
                    var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(':');
                    var attemptedLogin = credentials[0];
                    var attemptedPassword = credentials[1];

                    if (attemptedLogin == login && attemptedPassword == password)
                    {
                        await next();
                        return;
                    }
                }

                context.Response.StatusCode = 401;
                context.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{realm}\"";
                return;
            }

            await next();
        });
    }
}
