using System.Text.Json;

namespace Praxis.IntegrationTests.TestSupport;

/// <summary>
/// Finds the integration test connection string without demanding extra setup
/// from whoever cloned the repo. When there is none, integration tests are
/// skipped instead of failing — so `dotnet test` stays green for someone who has
/// not received credentials yet, and the push gate does not block for a reason
/// that is not their fault.
/// </summary>
public static class TestConfiguration
{
    private static readonly Lazy<string?> connection = new(Discover);

    public static string? ConnectionString => connection.Value;

    public static bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString);

    public const string SkipReason =
        "No test connection string. Set ConnectionStrings__IntegrationTests, or have "
        + "src/Praxis.Api/appsettings.Development.json filled in.";

    private static string? Discover()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("ConnectionStrings__IntegrationTests")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Primary");

        return !string.IsNullOrWhiteSpace(fromEnvironment) ? fromEnvironment : ReadFromLocalAppSettings();
    }

    private static string? ReadFromLocalAppSettings()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, "src", "Praxis.Api", "appsettings.Development.json");

            if (File.Exists(path))
            {
                return ReadConnection(path);
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string? ReadConnection(string path)
    {
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));

            if (!document.RootElement.TryGetProperty("ConnectionStrings", out var section))
            {
                return null;
            }

            if (section.TryGetProperty("IntegrationTests", out var tests)
                && !string.IsNullOrWhiteSpace(tests.GetString()))
            {
                return tests.GetString();
            }

            return section.TryGetProperty("Primary", out var primary) ? primary.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
