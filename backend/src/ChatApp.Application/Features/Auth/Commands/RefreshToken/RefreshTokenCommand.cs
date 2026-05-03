namespace ChatApp.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Request đổi refresh token hiện tại lấy cặp access/refresh token mới.
/// </summary>
public sealed class RefreshTokenCommand
{
    public string RefreshToken { get; init; } = string.Empty;
}
