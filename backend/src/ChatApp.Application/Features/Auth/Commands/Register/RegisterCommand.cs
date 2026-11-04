namespace ChatApp.Application.Features.Auth.Commands.Register;

/// <summary>
/// Request tạo tài khoản mới từ API, sau đó RegisterCommandHandler tạo User domain entity.
/// </summary>
public sealed class RegisterCommand
{
    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string? AvatarUrl { get; init; }
}
