using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Persistence;

/// <summary>
/// Contract truy cập dữ liệu Message cho các use case gửi, đọc, sửa, thu hồi hoặc xóa tin nhắn.
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

    /// <summary>
    /// Lấy danh sách tin nhắn theo phòng theo thứ tự thời gian tăng dần, hỗ trợ phân trang.
    /// </summary>
    Task<List<Message>> GetRoomMessagesAsync(Guid roomId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa hoàn toàn một tin nhắn khỏi cơ sở dữ liệu.
    /// </summary>
    Task DeleteAsync(Message message, CancellationToken cancellationToken = default);
}
