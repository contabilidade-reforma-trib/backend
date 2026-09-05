using Praxis.Shared.Abstractions;

namespace Praxis.Identity.Domain;

/// <summary>
/// The person who signs in. Registration data only — no organization, no
/// subscription, no profile. Those come back when the domain is settled.
/// </summary>
public sealed class User : EntityBase
{
    private User(Guid id, string name, string email, string? phone, DateTimeOffset createdAt)
        : base(id, createdAt)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }

    /// <summary>Used only by EF Core when materializing from the database.</summary>
    private User()
    {
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>Lowercased. It is the natural sign-in key and is unique system wide.</summary>
    public string Email { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    public static Result<User> Create(string name, string email, string? phone, IClock clock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<User>("user.empty_name", "Enter the user name.");
        }

        var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();

        if (!LooksLikeAnEmail(normalizedEmail))
        {
            return Result.Fail<User>("user.invalid_email", "Enter a valid email address.");
        }

        return Result.Ok(new User(Guid.NewGuid(), name.Trim(), normalizedEmail, Blank(phone), clock.UtcNow));
    }

    public void Rename(string name, IClock clock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        Name = name.Trim();
        Touch(clock);
    }

    public void UpdatePhone(string? phone, IClock clock)
    {
        Phone = Blank(phone);
        Touch(clock);
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool LooksLikeAnEmail(string email)
    {
        var at = email.IndexOf('@');
        var dot = email.LastIndexOf('.');

        return at > 0 && dot > at + 1 && dot < email.Length - 1;
    }
}
