using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
