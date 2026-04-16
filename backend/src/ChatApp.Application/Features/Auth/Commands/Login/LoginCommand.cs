namespace ChatApp.Application.Features.Auth.Commands.Login;

public sealed class LoginCommand
{
    public string EmailOrUserName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
