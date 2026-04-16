using ChatApp.Application.Common.Models;

namespace ChatApp.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(CurrentUser user, string? jwtId = null);
}
