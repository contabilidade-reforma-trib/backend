using Praxis.IntegrationTests.TestSupport;
using Praxis.Identity.Domain;
using Praxis.Identity.Infrastructure;
using Praxis.Shared.Abstractions;
using Xunit;

namespace Praxis.IntegrationTests.Identity;

[Collection(DatabaseCollection.Name)]
public class UserRepositoryTests
{
    private readonly TestDatabaseFixture database;
    private readonly IClock clock = new SystemClock();

    public UserRepositoryTests(TestDatabaseFixture database) => this.database = database;

    [IntegrationFact]
    public async Task Should_create_the_isolated_schema_with_every_table()
    {
        await using var context = database.CreateIdentityContext();
        Assert.True(await context.Database.CanConnectAsync());

        var tables = await ListTablesInSchema();

        Assert.Contains("identity_users", tables);
        Assert.Contains("copilot_documents", tables);
        Assert.Contains("copilot_chunks", tables);
        Assert.StartsWith("test_", database.Schema);
    }

    [IntegrationFact]
    public async Task Should_persist_and_read_back_a_user()
    {
        var email = $"aline.{Guid.NewGuid():N}@firm.com.br";
        var user = User.Create("Aline Bertoni", email, "41 99999-0000", clock).Value;

        await using (var write = database.CreateIdentityContext())
        {
            var repository = new UserRepository(write);
            await repository.Add(user, default);
            await repository.SaveChanges(default);
        }

        await using var read = database.CreateIdentityContext();
        var found = await new UserRepository(read).GetByEmail(email, default);

        Assert.NotNull(found);
        Assert.Equal("Aline Bertoni", found!.Name);
        Assert.Equal("41 99999-0000", found.Phone);
    }

    [IntegrationFact]
    public async Task Should_report_a_taken_email_regardless_of_casing()
    {
        var email = $"rafael.{Guid.NewGuid():N}@firm.com.br";
        var user = User.Create("Rafael", email, null, clock).Value;

        await using (var write = database.CreateIdentityContext())
        {
            var repository = new UserRepository(write);
            await repository.Add(user, default);
            await repository.SaveChanges(default);
        }

        await using var read = database.CreateIdentityContext();
        var repositoryForRead = new UserRepository(read);

        Assert.True(await repositoryForRead.EmailIsTaken(email.ToUpperInvariant(), default));
        Assert.False(await repositoryForRead.EmailIsTaken($"nobody.{Guid.NewGuid():N}@firm.com.br", default));
    }

    private async Task<List<string>> ListTablesInSchema()
    {
        await using var connection = new Npgsql.NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync();

        await using var command = new Npgsql.NpgsqlCommand(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = @schema",
            connection);
        command.Parameters.AddWithValue("schema", database.Schema);

        var tables = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }
}
