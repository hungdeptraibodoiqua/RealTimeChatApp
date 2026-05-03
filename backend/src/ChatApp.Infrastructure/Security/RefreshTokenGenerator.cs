using System.Security.Cryptography;
using System.Text;
using ChatApp.Application.Abstractions.Security;

namespace ChatApp.Infrastructure.Security;

/// <summary>
/// Sinh raw refresh token bảo mật và hash token trước khi lưu database.
/// </summary>
public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string GenerateToken()
    {
        // Raw token có entropy cao và chỉ được trả về client một lần.
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string HashToken(string token)
    {
        // Database chỉ lưu SHA-256 hash để nếu DB lộ thì raw refresh token không bị dùng trực tiếp.
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public string GenerateTokenFamily()
    {
        // TokenFamily nhóm các token cùng chuỗi rotate của một login session.
        return Guid.NewGuid().ToString("N");
    }

    public string GenerateJwtId()
    {
        // JwtId dùng làm jti claim và liên kết access token với refresh token.
        return Guid.NewGuid().ToString("N");
    }
}
