using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Persistence;

/// <summary>
/// Contract truy cập dữ liệu Room cho các use case tạo, truy vấn và quản trị phòng chat cùng các thành viên.
/// </summary>
public interface IRoomRepository
{
    /// <summary>
    /// Tìm phòng theo Id để kiểm tra tồn tại hoặc lấy thông tin phòng.
    /// </summary>
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm phòng kèm danh sách các thành viên để xử lý chuyển quyền Owner và kiểm tra vai trò.
    /// </summary>
    Task<Room?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các phòng chat mà user hiện tại đang là thành viên hoạt động.
    /// </summary>
    Task<List<Room>> GetUserRoomsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đưa room mới vào DbContext; UnitOfWork sẽ commit cùng các thay đổi liên quan.
    /// </summary>
    Task AddAsync(Room room, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm thành viên mới vào phòng.
    /// </summary>
    Task AddMemberAsync(RoomMember member, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin thành viên cụ thể trong phòng (để kiểm tra role, kick, leave).
    /// </summary>
    Task<RoomMember?> GetMemberAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách tất cả thành viên còn hoạt động của phòng.
    /// </summary>
    Task<List<RoomMember>> GetRoomMembersAsync(Guid roomId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa phòng khỏi cơ sở dữ liệu.
    /// </summary>
    Task DeleteAsync(Room room, CancellationToken cancellationToken = default);
}
