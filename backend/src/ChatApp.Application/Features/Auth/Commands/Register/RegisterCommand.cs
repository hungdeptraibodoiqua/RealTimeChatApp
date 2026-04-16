namespace ChatApp.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommand
{
    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
