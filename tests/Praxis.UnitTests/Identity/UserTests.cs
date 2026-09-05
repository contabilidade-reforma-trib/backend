using Praxis.Identity.Domain;
using Praxis.UnitTests.TestSupport;
using Xunit;

namespace Praxis.UnitTests.Identity;

public class UserTests
{
    private readonly FakeClock clock = FakeClock.On(2026, 9, 5);

    [Fact]
    public void Should_create_user_with_valid_data()
    {
        var result = User.Create("Aline Bertoni", "aline@firm.com.br", "41 99999-0000", clock);

        Assert.True(result.Succeeded);
        Assert.Equal("Aline Bertoni", result.Value.Name);
        Assert.Equal("41 99999-0000", result.Value.Phone);
    }

    [Fact]
    public void Should_lowercase_and_trim_the_email_because_it_is_the_sign_in_key()
    {
        var result = User.Create("Aline", "  Aline.Bertoni@Firm.COM.BR ", null, clock);

        Assert.True(result.Succeeded);
        Assert.Equal("aline.bertoni@firm.com.br", result.Value.Email);
    }

    [Theory]
    [InlineData("no-at-sign")]
    [InlineData("no@dot")]
    [InlineData("@domain.com")]
    [InlineData("")]
    public void Should_reject_invalid_email(string email)
    {
        var result = User.Create("Aline", email, null, clock);

        Assert.True(result.Failed);
        Assert.Equal("user.invalid_email", result.Error.Code);
    }

    [Fact]
    public void Should_reject_empty_name()
    {
        var result = User.Create("   ", "aline@firm.com.br", null, clock);

        Assert.True(result.Failed);
        Assert.Equal("user.empty_name", result.Error.Code);
    }

    [Fact]
    public void Should_treat_blank_phone_as_absent()
    {
        var result = User.Create("Aline", "aline@firm.com.br", "   ", clock);

        Assert.Null(result.Value.Phone);
    }

    [Fact]
    public void Should_touch_updated_at_when_phone_changes()
    {
        var user = User.Create("Aline", "aline@firm.com.br", null, clock).Value;
        var before = user.UpdatedAt;

        clock.Advance(TimeSpan.FromHours(2));
        user.UpdatePhone("41 98888-1111", clock);

        Assert.Equal("41 98888-1111", user.Phone);
        Assert.True(user.UpdatedAt > before);
    }
}
