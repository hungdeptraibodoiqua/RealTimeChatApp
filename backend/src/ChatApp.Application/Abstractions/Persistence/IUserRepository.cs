using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Persistence;

/// <summary>
/// Contract truy cập dữ liệu User cho Application; implementation cụ thể nằm ở Infrastructure.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Tìm user theo Id để các use case lấy lại account hiện có.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm user theo email để phục vụ login và kiểm tra trùng khi register.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm user theo username để phục vụ login và kiểm tra trùng khi register.
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đưa user mới vào DbContext; việc commit do IUnitOfWork xử lý.
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
