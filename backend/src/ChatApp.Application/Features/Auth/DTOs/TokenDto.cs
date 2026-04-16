namespace ChatApp.Application.Features.Auth.DTOs;

public sealed class TokenDto
{
    public string AccessToken { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    // JwtId là token artifact nên vẫn giữ string.
    public string? JwtId { get; init; }
}
