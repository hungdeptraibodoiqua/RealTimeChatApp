using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Persistence;

/// <summary>
/// Contract truy cập refresh token đã hash để login session và token rotation không phụ thuộc EF Core.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Tìm refresh token theo Id, thường dùng khi liên kết token cũ và token thay thế.
    /// </summary>
    Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm token bằng hash của raw token client gửi lên, không truy vấn bằng raw token.
    /// </summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đưa refresh token mới vào DbContext; UnitOfWork chịu trách nhiệm commit.
    /// </summary>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
