using ChatApp.Application.Abstractions.Security;

namespace ChatApp.Infrastructure.Security;

/// <summary>
/// BCrypt implementation cho IPasswordHasher, dùng để lưu password hash và verify khi login.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        // Hash password trước khi lưu vào User.PasswordHash, không lưu raw password.
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        // So sánh raw password từ request login với hash đã lưu trong database.
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
