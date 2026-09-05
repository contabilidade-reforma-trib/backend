using Amazon.Runtime;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Praxis.Copilot.Application;
using Praxis.Copilot.Infrastructure;
using Praxis.Identity.Application;
using Praxis.Identity.Infrastructure;
using Praxis.Shared.Abstractions;
using Praxis.Shared.Ai;
using Praxis.Shared.Storage;

namespace Praxis.Api.Configuration;

public static class ServiceConfiguration
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Primary");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Primary is not configured. Locally it comes from "
                + "appsettings.Development.json; on Railway, from the environment variable "
                + "ConnectionStrings__Primary.");
        }

        // One context per module, same database. Each keeps its own migration
        // history table so the two never collide.
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable(IdentityDbContext.MigrationsHistoryTable)));

        services.AddDbContext<CopilotDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.UseVector().MigrationsHistoryTable(CopilotDbContext.MigrationsHistoryTable)));

        services.AddSingleton<IClock, SystemClock>();

        return services;
    }

    public static IServiceCollection AddAi(this IServiceCollection services, IConfiguration configuration)
    {
        var apiKey = configuration["Ai:ApiKey"];

        // No key, no vendor call. The stand-in keeps ingestion and retrieval
        // runnable end to end; swapping it for a real provider is one line here.
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            services.AddSingleton<IAiProvider, DeterministicAiProvider>();
        }
        else
        {
            throw new NotImplementedException(
                "Ai:ApiKey is set but no real provider is implemented yet. "
                + "Remove the key to fall back to DeterministicAiProvider.");
        }

        return services;
    }

    public static IServiceCollection AddObjectStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.Section));

        var options = configuration.GetSection(StorageOptions.Section).Get<StorageOptions>()
            ?? new StorageOptions();

        // Without credentials the API still starts and the health check reports
        // storage as unconfigured. Better than failing at boot over something
        // that is not contracted yet.
        if (!options.IsConfigured)
        {
            return services;
        }

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var clientConfiguration = new AmazonS3Config
            {
                ServiceURL = options.Endpoint,
                // R2 requires path style addressing, not a subdomain per bucket.
                ForcePathStyle = true,
                AuthenticationRegion = options.Region,
            };

            var credentials = new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey);
            return new AmazonS3Client(credentials, clientConfiguration);
        });

        services.AddSingleton<IObjectStorage, R2ObjectStorage>();

        return services;
    }

    public static IServiceCollection AddModules(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IKnowledgeRepository, KnowledgeRepository>();
        services.AddScoped<IngestDocument>();
        services.AddScoped<SearchKnowledge>();

        return services;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Praxis API",
                Version = "v1",
                Description =
                    "Tax copilot and mentorship for Brazilian accountants. "
                    + "Prototype — data and credentials are disposable.",
            });
        });

        return services;
    }
}
