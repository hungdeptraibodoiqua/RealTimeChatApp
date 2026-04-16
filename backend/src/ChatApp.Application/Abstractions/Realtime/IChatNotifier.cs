namespace ChatApp.Application.Abstractions.Realtime;

public interface IChatNotifier
{
    Task UserOnlineAsync(Guid userId, CancellationToken cancellationToken = default);

    Task UserOfflineAsync(Guid userId, CancellationToken cancellationToken = default);

    Task MessageCreatedAsync(Guid roomId, Guid messageId, CancellationToken cancellationToken = default);
}
