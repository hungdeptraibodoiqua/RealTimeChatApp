using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Persistence;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Room room, CancellationToken cancellationToken = default);
}
