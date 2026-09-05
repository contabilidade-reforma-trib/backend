using Microsoft.EntityFrameworkCore;
using Npgsql;
using Praxis.Copilot.Infrastructure;
using Praxis.Identity.Infrastructure;
using Xunit;

namespace Praxis.IntegrationTests.TestSupport;

/// <summary>
/// Creates an isolated schema per run in the same Neon database, builds the
/// tables inside it, and drops it at the end — including when a test fails or
/// blows up midway, because the drop lives in a <c>finally</c>.
///
/// The name carries the creation instant (<c>test_yyyyMMddHHmmss_xxxx</c>) so the
/// orphan sweep can tell how old a schema is: if the machine dies before dispose,
/// the next run cleans up what was left behind.
/// </summary>
public sealed class TestDatabaseFixture : IAsyncLifetime
{
    private const string SchemaPrefix = "test_";
    private static readonly TimeSpan MaxOrphanAge = TimeSpan.FromHours(24);

    public string Schema { get; } =
        $"{SchemaPrefix}{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}"[..38];

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        if (!TestConfiguration.IsConfigured)
        {
            return;
        }

        var baseConnection = TestConfiguration.ConnectionString!;

        await DropOrphanSchemas(baseConnection);
        await Execute(baseConnection, $"CREATE SCHEMA IF NOT EXISTS \"{Schema}\";");

        // pgvector installs into public. The test schema comes first in the
        // search path so tables land there, but public has to stay reachable or
        // the `vector` type cannot be resolved.
        ConnectionString = $"{baseConnection.TrimEnd(';')};Search Path={Schema},public";

        // EnsureCreated is useless here: it decides by whether the DATABASE
        // exists, and it does. The new schema would stay empty and every insert
        // would fail with "relation does not exist". Generating the model script
        // creates the tables inside the isolated schema, which is what we want.
        //
        // One script per module context, because each owns its own tables.
        await using (var identity = CreateIdentityContext())
        {
            await Execute(ConnectionString, identity.Database.GenerateCreateScript());
        }

        await using var copilot = CreateCopilotContext();
        await Execute(ConnectionString, copilot.Database.GenerateCreateScript());
    }

    public async Task DisposeAsync()
    {
        if (!TestConfiguration.IsConfigured)
        {
            return;
        }

        try
        {
            await Execute(TestConfiguration.ConnectionString!, $"DROP SCHEMA IF EXISTS \"{Schema}\" CASCADE;");
        }
        catch (NpgsqlException)
        {
            // Failing here must not take the suite down. The schema becomes an
            // orphan and the next run's sweep deals with it.
        }
    }

    public IdentityDbContext CreateIdentityContext()
    {
        var builder = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(ConnectionString);

        ApplyOptionalSqlLog(builder);
        return new IdentityDbContext(builder.Options, Schema);
    }

    public CopilotDbContext CreateCopilotContext()
    {
        var builder = new DbContextOptionsBuilder<CopilotDbContext>()
            .UseNpgsql(ConnectionString, npgsql => npgsql.UseVector());

        ApplyOptionalSqlLog(builder);
        return new CopilotDbContext(builder.Options, Schema);
    }

    /// <summary>Diagnóstico opcional: PRAXIS_SQL_LOG=&lt;caminho&gt; grava o SQL emitido.</summary>
    private static void ApplyOptionalSqlLog(DbContextOptionsBuilder builder)
    {
        var sqlLog = Environment.GetEnvironmentVariable("PRAXIS_SQL_LOG");

        if (string.IsNullOrWhiteSpace(sqlLog))
        {
            return;
        }

        builder.LogTo(
            message => File.AppendAllText(sqlLog, message + Environment.NewLine),
            Microsoft.Extensions.Logging.LogLevel.Information);
    }

    private static async Task DropOrphanSchemas(string connectionString)
    {
        var cutoff = DateTime.UtcNow - MaxOrphanAge;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var names = new List<string>();

        await using (var command = new NpgsqlCommand(
            "SELECT schema_name FROM information_schema.schemata WHERE schema_name LIKE @prefix",
            connection))
        {
            command.Parameters.AddWithValue("prefix", $"{SchemaPrefix}%");
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                names.Add(reader.GetString(0));
            }
        }

        foreach (var name in names.Where(name => IsOrphan(name, cutoff)))
        {
            await using var command = new NpgsqlCommand($"DROP SCHEMA IF EXISTS \"{name}\" CASCADE;", connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    private static bool IsOrphan(string schemaName, DateTime cutoff)
    {
        var parts = schemaName.Split('_');

        if (parts.Length < 3
            || !DateTime.TryParseExact(
                parts[1],
                "yyyyMMddHHmmss",
                null,
                System.Globalization.DateTimeStyles.AssumeUniversal
                    | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var createdAt))
        {
            return false;
        }

        return createdAt < cutoff;
    }

    private static async Task Execute(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
