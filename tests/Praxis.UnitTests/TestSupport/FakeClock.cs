using Praxis.Shared.Abstractions;

namespace Praxis.UnitTests.TestSupport;

/// <summary>Clock the test controls, so validity windows can be checked without waiting.</summary>
public sealed class FakeClock : IClock
{
    public FakeClock(DateTimeOffset now) => UtcNow = now;

    public DateTimeOffset UtcNow { get; private set; }

    public static FakeClock On(int year, int month, int day) =>
        new(new DateTimeOffset(year, month, day, 12, 0, 0, TimeSpan.Zero));

    public void Advance(TimeSpan period) => UtcNow = UtcNow.Add(period);
}
