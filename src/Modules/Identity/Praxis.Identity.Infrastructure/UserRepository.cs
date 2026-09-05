using Microsoft.EntityFrameworkCore;
using Praxis.Identity.Application;
using Praxis.Identity.Domain;


namespace Praxis.Identity.Infrastructure;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext context;

    public UserRepository(IdentityDbContext context) => this.context = context;

    public Task<User?> GetById(Guid userId, CancellationToken cancellationToken) =>
        context.Users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public Task<User?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return context.Users.FirstOrDefaultAsync(user => user.Email == normalized, cancellationToken);
    }

    public Task<bool> EmailIsTaken(string email, CancellationToken cancellationToken)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return context.Users.AnyAsync(user => user.Email == normalized, cancellationToken);
    }

    /// <summary>Paged from the start: a user list is exactly the kind that grows.</summary>
    public async Task<IReadOnlyCollection<User>> List(int skip, int take, CancellationToken cancellationToken) =>
        await context.Users
            .OrderBy(user => user.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task Add(User user, CancellationToken cancellationToken) =>
        await context.Users.AddAsync(user, cancellationToken);

    public Task SaveChanges(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
