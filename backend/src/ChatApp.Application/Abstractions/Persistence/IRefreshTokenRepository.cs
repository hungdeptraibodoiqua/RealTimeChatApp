using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
