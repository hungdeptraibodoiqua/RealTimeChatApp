using ChatApp.Application.Common.Models;

namespace ChatApp.Application.Abstractions.Security;

/// <summary>
/// Contract phát access token JWT cho user đã xác thực.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Tạo access token chứa user id, email, username và jti để API nhận diện request sau này.
    /// </summary>
    string GenerateAccessToken(CurrentUser user, string? jwtId = null);

    /// <summary>
    /// Tính thời điểm access token hết hạn theo cấu hình hiện tại.
    /// </summary>
    DateTime GetAccessTokenExpiresAtUtc();
}
