using ChatApp.Application.Common.Models;

namespace ChatApp.Application.Features.Auth.DTOs;

/// <summary>
/// Response trả về sau register/login/refresh, gồm thông tin user và token nếu use case có phát token.
/// </summary>
public sealed class AuthResponseDto : BaseDto
{
    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public TokenDto Tokens { get; init; } = new();
}
