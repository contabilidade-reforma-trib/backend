namespace Praxis.Shared.Abstractions;

/// <summary>
/// Time enters through here, never through <c>DateTimeOffset.UtcNow</c> directly.
/// It is what lets us test subscription validity without waiting on the calendar.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
