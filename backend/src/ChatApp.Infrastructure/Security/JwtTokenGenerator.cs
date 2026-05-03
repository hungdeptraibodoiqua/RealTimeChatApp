using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatApp.Application.Abstractions.Security;
using ChatApp.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ChatApp.Infrastructure.Security;

/// <summary>
/// Sinh access token JWT dựa trên cấu hình Jwt:* và thông tin CurrentUser từ Application.
/// </summary>
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(CurrentUser user, string? jwtId = null)
    {
        var secret = GetRequiredSetting("Jwt:Secret");
        var issuer = GetRequiredSetting("Jwt:Issuer");
        var audience = GetRequiredSetting("Jwt:Audience");
        var expiresAtUtc = GetAccessTokenExpiresAtUtc();

        var claims = new List<Claim>
        {
            // sub/nameidentifier là user id để API đọc lại identity từ JWT.
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            // Email được thêm bằng cả chuẩn JWT và ClaimTypes để tương thích middleware/thư viện khác nhau.
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            // username là claim custom để client/API có thể đọc nhanh tên đăng nhập.
            new("username", user.Username),
            // jti định danh access token, dùng để liên kết với refresh token trong auth flow.
            new(JwtRegisteredClaimNames.Jti, string.IsNullOrWhiteSpace(jwtId) ? Guid.NewGuid().ToString("N") : jwtId)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetAccessTokenExpiresAtUtc()
    {
        var minutes = _configuration.GetValue<int?>("Jwt:AccessTokenExpirationMinutes") ?? 15;
        return DateTime.UtcNow.AddMinutes(minutes);
    }

    private string GetRequiredSetting(string key)
    {
        // Cấu hình JWT thiếu là lỗi startup/runtime nghiêm trọng nên fail fast thay vì phát token sai.
        return _configuration[key]
            ?? throw new InvalidOperationException($"Configuration value '{key}' was not found.");
    }
}
