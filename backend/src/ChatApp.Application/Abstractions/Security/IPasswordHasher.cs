namespace ChatApp.Application.Abstractions.Security;

/// <summary>
/// Contract hash và verify password để Application không phụ thuộc trực tiếp BCrypt hay thư viện cụ thể.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Chuyển raw password thành hash trước khi lưu vào User.PasswordHash.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// So sánh raw password đăng nhập với hash đã lưu trong database.
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}
