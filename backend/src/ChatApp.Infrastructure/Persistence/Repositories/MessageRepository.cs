using ChatApp.Application.Abstractions.Persistence;
using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Thực thi truy vấn và quản lý dữ liệu Message thông qua AppDbContext.
/// Phục vụ luồng gửi tin nhắn, tải lịch sử tin nhắn phòng, sửa, thu hồi và xóa tin nhắn.
/// </summary>
public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        await _context.Messages.AddAsync(message, cancellationToken);
    }

    public async Task<List<Message>> GetRoomMessagesAsync(Guid roomId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        // Lấy tin nhắn của phòng, lọc các tin nhắn đã soft delete (DeletedAtUtc == null), sắp xếp tăng dần theo thời gian
        return await _context.Messages
            .Where(m => m.RoomId == roomId && m.DeletedAtUtc == null)
            .OrderBy(m => m.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task DeleteAsync(Message message, CancellationToken cancellationToken = default)
    {
        _context.Messages.Remove(message);
        return Task.CompletedTask;
    }
}
