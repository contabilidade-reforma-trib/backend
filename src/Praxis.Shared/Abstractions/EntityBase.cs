namespace Praxis.Shared.Abstractions;

/// <summary>Base of every persisted entity. Timestamps are always UTC.</summary>
public abstract class EntityBase
{
    protected EntityBase(Guid id, DateTimeOffset createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>Used only by EF Core when materializing from the database.</summary>
    protected EntityBase()
    {
    }

    public Guid Id { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    protected void Touch(IClock clock) => UpdatedAt = clock.UtcNow;
}
