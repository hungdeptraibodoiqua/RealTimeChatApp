using ChatApp.Application.Common.Models;

namespace ChatApp.Application.Features.Auth.DTOs;

public sealed class AuthResponseDto : BaseDto
{
    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public TokenDto Tokens { get; init; } = new();
}
