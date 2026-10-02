using ChatApp.Application.Abstractions.Persistence;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Thực thi truy vấn và quản lý dữ liệu Friendship thông qua AppDbContext.
/// Phục vụ luồng gửi yêu cầu kết bạn, chấp nhận/từ chối và lấy danh sách bạn bè.
/// </summary>
public class FriendshipRepository : IFriendshipRepository
{
    private readonly AppDbContext _context;

    public FriendshipRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Friendship?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Friendships
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Friendship?> GetFriendshipAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default)
    {
        // Kiểm tra quan hệ 2 chiều giữa 2 user
        return await _context.Friendships
            .FirstOrDefaultAsync(f =>
                (f.RequesterId == user1Id && f.AddresseeId == user2Id) ||
                (f.RequesterId == user2Id && f.AddresseeId == user1Id),
                cancellationToken);
    }

    public async Task<List<Friendship>> GetUserFriendsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Lấy tất cả quan hệ Accepted có dính tới userId
        return await _context.Friendships
            .Where(f => (f.RequesterId == userId || f.AddresseeId == userId) && f.Status == FriendshipStatus.Accepted)
            .OrderByDescending(f => f.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Friendship>> GetPendingRequestsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Lấy các lời mời kết bạn gửi tới user này (AddresseeId == userId) đang ở trạng thái Pending
        return await _context.Friendships
            .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
            .OrderByDescending(f => f.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Friendship friendship, CancellationToken cancellationToken = default)
    {
        await _context.Friendships.AddAsync(friendship, cancellationToken);
    }
}
