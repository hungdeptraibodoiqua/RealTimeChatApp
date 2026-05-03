using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;

namespace ChatApp.Application.Abstractions.Persistence;

/// <summary>
/// Contract truy cập dữ liệu Message cho các use case gửi, đọc, sửa hoặc xóa tin nhắn.
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// Tìm message theo Id để kiểm tra quyền thao tác hoặc tạo reply reference.
    /// </summary>
    Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đưa message mới vào DbContext; repository không tự commit.
    /// </summary>
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
}
