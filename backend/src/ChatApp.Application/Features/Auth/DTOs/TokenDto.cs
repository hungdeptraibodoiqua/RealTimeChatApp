namespace ChatApp.Application.Features.Auth.DTOs;

/// <summary>
/// DTO chứa cặp token trả về client; refresh token ở đây là raw token chỉ xuất hiện trong response.
/// </summary>
public sealed class TokenDto
{
    public string AccessToken { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }

    // JwtId là token artifact nên vẫn giữ string.
    public string? JwtId { get; init; }
}
