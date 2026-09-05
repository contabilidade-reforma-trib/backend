namespace Praxis.Api.Configuration;

/// <summary>
/// No browser calls this API directly: the frontend uses the Next server as a
/// BFF, and server-to-server calls do not go through CORS.
///
/// The policy stays as an escape hatch for a one-off case, and that is why an
/// empty list means NO origin allowed. Opening it up in the silence of missing
/// configuration would expose the backend to any site without anyone deciding so.
/// </summary>
public static class CorsConfiguration
{
    public const string PolicyName = "frontend";

    public static IServiceCollection AddFrontendCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options => options.AddPolicy(PolicyName, policy =>
        {
            if (origins.Length == 0)
            {
                return;
            }

            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }));

        return services;
    }
}
