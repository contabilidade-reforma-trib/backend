using Praxis.Identity.Domain;

namespace Praxis.Identity.Application;

public interface IUserRepository
{
    Task<User?> GetById(Guid userId, CancellationToken cancellationToken);

    Task<User?> GetByEmail(string email, CancellationToken cancellationToken);

    Task<bool> EmailIsTaken(string email, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<User>> List(int skip, int take, CancellationToken cancellationToken);

    Task Add(User user, CancellationToken cancellationToken);

    Task SaveChanges(CancellationToken cancellationToken);
}
