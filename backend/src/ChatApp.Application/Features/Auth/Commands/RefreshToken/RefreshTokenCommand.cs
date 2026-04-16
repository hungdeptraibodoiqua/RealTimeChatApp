namespace ChatApp.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommand
{
    public string RefreshToken { get; init; } = string.Empty;
}
