using ChatApp.Application.Abstractions.Persistence;
using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Thực thi truy vấn và quản lý dữ liệu Room và RoomMember thông qua AppDbContext.
/// Phục vụ luồng nghiệp vụ tạo phòng, lấy danh sách phòng, thêm/xóa thành viên và chuyển quyền Owner.
/// </summary>
public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Room?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Load Room và kèm các RoomMembers liên kết
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (room != null)
        {
            await _context.RoomMembers
                .Where(m => m.RoomId == id)
                .LoadAsync(cancellationToken);
        }

        return room;
    }

    public async Task<List<Room>> GetUserRoomsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Lấy danh sách room mà user đang tham gia và chưa rời phòng (LeftAtUtc == null)
        var roomIds = await _context.RoomMembers
            .Where(m => m.UserId == userId && m.LeftAtUtc == null)
            .Select(m => m.RoomId)
            .ToListAsync(cancellationToken);

        return await _context.Rooms
            .Where(r => roomIds.Contains(r.Id))
            .OrderByDescending(r => r.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Room room, CancellationToken cancellationToken = default)
    {
        await _context.Rooms.AddAsync(room, cancellationToken);
    }

    public async Task AddMemberAsync(RoomMember member, CancellationToken cancellationToken = default)
    {
        await _context.RoomMembers.AddAsync(member, cancellationToken);
    }

    public async Task<RoomMember?> GetMemberAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.RoomMembers
            .FirstOrDefaultAsync(m => m.RoomId == roomId && m.UserId == userId, cancellationToken);
    }

    public async Task<List<RoomMember>> GetRoomMembersAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return await _context.RoomMembers
            .Where(m => m.RoomId == roomId && m.LeftAtUtc == null)
            .ToListAsync(cancellationToken);
    }

    public Task DeleteAsync(Room room, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Remove(room);
        return Task.CompletedTask;
    }
}
