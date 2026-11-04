namespace ChatApp.Application.Features.Auth.Commands.Login;

/// <summary>
/// Request đăng nhập bằng email hoặc username kèm password.
/// </summary>
public sealed class LoginCommand
{
    public string EmailOrUserName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
