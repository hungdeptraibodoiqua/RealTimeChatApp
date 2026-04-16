namespace ChatApp.Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommand
{
    public Guid UserId { get; init; }

    public string? JwtId { get; init; }
}
