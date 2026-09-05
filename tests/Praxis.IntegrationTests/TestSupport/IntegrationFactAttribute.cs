using Xunit;

namespace Praxis.IntegrationTests.TestSupport;

/// <summary>
/// Like <see cref="FactAttribute"/>, but skips itself when there is no test
/// database configured. xUnit 2 decides the skip when the attribute is built,
/// which is why the check lives here and not inside the test.
/// </summary>
public sealed class IntegrationFactAttribute : FactAttribute
{
    public IntegrationFactAttribute()
    {
        if (!TestConfiguration.IsConfigured)
        {
            Skip = TestConfiguration.SkipReason;
        }
    }
}
