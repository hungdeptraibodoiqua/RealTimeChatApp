namespace ChatApp.Application.Abstractions.Security;

/// <summary>
/// Contract sinh raw refresh token, token hash và các định danh phục vụ token rotation.
/// </summary>
public interface IRefreshTokenGenerator
{
    /// <summary>
    /// Sinh raw refresh token để trả về client đúng một lần.
    /// </summary>
    string GenerateToken();

    /// <summary>
    /// Hash raw refresh token trước khi lưu database.
    /// </summary>
    string HashToken(string token);

    /// <summary>
    /// Sinh mã family để nhóm các refresh token cùng chuỗi rotate.
    /// </summary>
    string GenerateTokenFamily();

    /// <summary>
    /// Sinh jti liên kết access token với refresh token tương ứng.
    /// </summary>
    string GenerateJwtId();
}
