namespace ChatApp.Application.Abstractions.Security;

public interface IRefreshTokenGenerator
{
    string GenerateToken();

    string GenerateTokenFamily();

    string GenerateJwtId();
}
