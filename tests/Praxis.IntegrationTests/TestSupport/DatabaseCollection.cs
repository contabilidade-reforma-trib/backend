using Xunit;

namespace Praxis.IntegrationTests.TestSupport;

/// <summary>
/// One schema for the whole run, shared by every integration test class.
///
/// With IClassFixture each class would build its own schema, which is slower and
/// lets classes race while creating and dropping schemas in the same database.
/// A collection fixture also serialises the classes, so tests never fight over
/// the same rows.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<TestDatabaseFixture>
{
    public const string Name = "database";
}
