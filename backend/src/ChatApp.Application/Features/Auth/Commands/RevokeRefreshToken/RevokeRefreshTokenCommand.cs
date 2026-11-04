namespace ChatApp.Application.Features.Auth.Commands.RevokeRefreshToken;

public sealed class RevokeRefreshTokenCommand
{
    public Guid UserId { get; init; }

    public string RefreshToken { get; init; } = string.Empty;
}
