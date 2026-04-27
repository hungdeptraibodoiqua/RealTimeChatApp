using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;

namespace ChatApp.Application.Abstractions.Persistence;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Message message, CancellationToken cancellationToken = default);
}
