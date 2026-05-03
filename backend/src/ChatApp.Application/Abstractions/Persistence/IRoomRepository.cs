using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Persistence;

/// <summary>
/// Contract truy cập dữ liệu Room cho các use case tạo và quản lý phòng chat.
/// </summary>
public interface IRoomRepository
{
    /// <summary>
    /// Tìm phòng theo Id để kiểm tra tồn tại hoặc lấy thông tin phòng.
    /// </summary>
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đưa room mới vào DbContext; UnitOfWork sẽ commit cùng các thay đổi liên quan.
    /// </summary>
    Task AddAsync(Room room, CancellationToken cancellationToken = default);
}
