using System.Security.Claims;
using ChatApp.Application.Abstractions.Persistence;
using ChatApp.Application.Abstractions.Realtime;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

/// <summary>
/// Controller quản lý phòng chat: tạo phòng, thêm/xóa thành viên, chuyển Owner và tự động chuyển Owner khi thoát phòng.
/// Kết nối với: RoomRepository, UserRepository, IChatNotifier, AppDbContext/UnitOfWork.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatNotifier _chatNotifier;

    public RoomsController(
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IChatNotifier chatNotifier)
    {
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _chatNotifier = chatNotifier;
    }

    /// <summary>
    /// Lấy danh sách tất cả các phòng mà user hiện tại đang tham gia.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyRooms()
    {
        var currentUserId = GetCurrentUserId();
        var rooms = await _roomRepository.GetUserRoomsAsync(currentUserId);

        var dtos = rooms.Select(r => new
        {
            r.Id,
            r.Name,
            Type = (int)r.Type,
            r.CreatedByUserId,
            r.CreatedAtUtc,
            r.UpdatedAtUtc
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Lấy danh sách thành viên trong một phòng cụ thể kèm thông tin người dùng.
    /// </summary>
    [HttpGet("{roomId:guid}/members")]
    public async Task<IActionResult> GetRoomMembers(Guid roomId)
    {
        var currentUserId = GetCurrentUserId();
        var currentMember = await _roomRepository.GetMemberAsync(roomId, currentUserId);
        if (currentMember == null || currentMember.LeftAtUtc != null)
            return Forbid("Bạn không phải thành viên của phòng này.");

        var members = await _roomRepository.GetRoomMembersAsync(roomId);
        var memberDtos = new List<object>();

        foreach (var m in members)
        {
            var user = await _userRepository.GetByIdAsync(m.UserId);
            memberDtos.Add(new
            {
                m.UserId,
                m.RoomId,
                Role = (int)m.Role,
                RoleName = m.Role.ToString(),
                m.JoinedAtUtc,
                DisplayName = user?.DisplayName ?? "Người dùng",
                Username = user?.Username ?? "user",
                AvatarUrl = user?.AvatarUrl
            });
        }

        return Ok(memberDtos);
    }

    /// <summary>
    /// Tạo phòng chat mới (Public/Private/Group/Direct) và tự động gán người tạo làm Owner.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto request)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { detail = "Tên phòng không được để trống." });

        var roomType = Enum.IsDefined(typeof(RoomType), request.Type)
            ? (RoomType)request.Type
            : RoomType.Group;

        var roomId = Guid.NewGuid();
        var room = new Room(roomId, request.Name, roomType, currentUserId);
        await _roomRepository.AddAsync(room);

        // Gán người tạo phòng làm Owner
        var ownerMember = new RoomMember(currentUserId, roomId, MemberRole.Owner);
        await _roomRepository.AddMemberAsync(ownerMember);

        // Nếu có danh sách thành viên mời ban đầu
        if (request.InitialMemberIds != null && request.InitialMemberIds.Any())
        {
            foreach (var memberId in request.InitialMemberIds.Distinct())
            {
                if (memberId != currentUserId)
                {
                    var newMember = new RoomMember(memberId, roomId, MemberRole.Member);
                    await _roomRepository.AddMemberAsync(newMember);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            room.Id,
            room.Name,
            Type = (int)room.Type,
            room.CreatedByUserId,
            room.CreatedAtUtc,
            room.UpdatedAtUtc
        });
    }

    /// <summary>
    /// Thêm một thành viên mới vào phòng (Chỉ Owner hoặc Admin có quyền thêm).
    /// </summary>
    [HttpPost("{roomId:guid}/members")]
    public async Task<IActionResult> AddMember(Guid roomId, [FromBody] AddMemberDto request)
    {
        var currentUserId = GetCurrentUserId();
        var requesterMember = await _roomRepository.GetMemberAsync(roomId, currentUserId);
        if (requesterMember == null || !requesterMember.IsOwnerOrAdmin())
            return Forbid("Chỉ Chủ phòng hoặc Quản trị viên mới có quyền thêm thành viên.");

        var existing = await _roomRepository.GetMemberAsync(roomId, request.UserId);
        if (existing != null && existing.LeftAtUtc == null)
            return BadRequest(new { detail = "Người dùng này đã là thành viên trong phòng." });

        var targetUser = await _userRepository.GetByIdAsync(request.UserId);
        if (targetUser == null)
            return NotFound(new { detail = "Người dùng không tồn tại." });

        var newMember = new RoomMember(request.UserId, roomId, MemberRole.Member);
        await _roomRepository.AddMemberAsync(newMember);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            newMember.UserId,
            newMember.RoomId,
            Role = (int)newMember.Role,
            DisplayName = targetUser.DisplayName
        });
    }

    /// <summary>
    /// Xóa (kick) một thành viên khỏi phòng chat (Chỉ Owner hoặc Admin có quyền xóa, Admin không thể xóa Owner).
    /// </summary>
    [HttpDelete("{roomId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid roomId, Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var requester = await _roomRepository.GetMemberAsync(roomId, currentUserId);
        if (requester == null || !requester.IsOwnerOrAdmin())
            return Forbid("Chỉ Chủ phòng hoặc Quản trị viên mới có quyền xóa thành viên.");

        var targetMember = await _roomRepository.GetMemberAsync(roomId, userId);
        if (targetMember == null || targetMember.LeftAtUtc != null)
            return NotFound(new { detail = "Thành viên không tồn tại trong phòng." });

        if (targetMember.IsOwner())
            return BadRequest(new { detail = "Không thể xóa Chủ phòng." });

        targetMember.Leave();
        await _unitOfWork.SaveChangesAsync();

        // Báo cho các client biết thành viên đã bị xóa
        await _chatNotifier.MemberRemovedAsync(roomId, userId);

        return Ok(new { message = "Đã xóa thành viên khỏi phòng." });
    }

    /// <summary>
    /// Chuyển quyền Chủ phòng (Owner) cho một thành viên khác trong phòng.
    /// </summary>
    [HttpPost("{roomId:guid}/transfer-owner")]
    public async Task<IActionResult> TransferOwner(Guid roomId, [FromBody] TransferOwnerDto request)
    {
        var currentUserId = GetCurrentUserId();
        var room = await _roomRepository.GetByIdWithMembersAsync(roomId);
        if (room == null)
            return NotFound(new { detail = "Phòng không tồn tại." });

        var currentOwner = await _roomRepository.GetMemberAsync(roomId, currentUserId);
        if (currentOwner == null || !currentOwner.IsOwner())
            return Forbid("Chỉ Chủ phòng hiện tại mới có quyền chuyển giao quyền sở hữu.");

        var targetMember = await _roomRepository.GetMemberAsync(roomId, request.NewOwnerUserId);
        if (targetMember == null || targetMember.LeftAtUtc != null)
            return BadRequest(new { detail = "Thành viên được chỉ định không tồn tại hoặc đã rời phòng." });

        // Chuyển role: Chủ cũ hạ thành Admin, người được chỉ định lên làm Owner
        currentOwner.ChangeRole(MemberRole.Admin);
        targetMember.ChangeRole(MemberRole.Owner);

        await _unitOfWork.SaveChangesAsync();

        // Phát sự kiện realtime thông báo toàn phòng
        await _chatNotifier.OwnerTransferredAsync(roomId, currentUserId, targetMember.UserId);

        return Ok(new { message = "Đã chuyển quyền Chủ phòng thành công.", newOwnerId = targetMember.UserId });
    }

    /// <summary>
    /// Rời phòng chat. Nếu người rời là Owner, hệ thống tự động gán quyền Owner cho thành viên kế tiếp (Admin hoặc tham gia lâu nhất).
    /// </summary>
    [HttpPost("{roomId:guid}/leave")]
    public async Task<IActionResult> LeaveRoom(Guid roomId)
    {
        var currentUserId = GetCurrentUserId();
        var room = await _roomRepository.GetByIdWithMembersAsync(roomId);
        if (room == null)
            return NotFound(new { detail = "Phòng không tồn tại." });

        var member = await _roomRepository.GetMemberAsync(roomId, currentUserId);
        if (member == null || member.LeftAtUtc != null)
            return BadRequest(new { detail = "Bạn không phải thành viên hoạt động của phòng này." });

        member.Leave();

        // Quy tắc nghiệp vụ tự động chuyển quyền Owner khi Owner rời phòng
        if (member.IsOwner())
        {
            var activeMembers = (await _roomRepository.GetRoomMembersAsync(roomId))
                .Where(m => m.UserId != currentUserId && m.LeftAtUtc == null)
                .OrderByDescending(m => m.Role == MemberRole.Admin) // Ưu tiên Admin trước
                .ThenBy(m => m.JoinedAtUtc)                        // Ưu tiên người tham gia lâu nhất
                .ToList();

            if (activeMembers.Any())
            {
                var successor = activeMembers.First();
                successor.ChangeRole(MemberRole.Owner);
                await _chatNotifier.OwnerTransferredAsync(roomId, currentUserId, successor.UserId);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        // Báo cho các client khác trong phòng biết user đã rời đi
        await _chatNotifier.UserLeftRoomAsync(roomId, currentUserId);

        return Ok(new { message = "Bạn đã rời phòng thành công." });
    }

    /// <summary>
    /// Xóa toàn bộ phòng chat (Chỉ Owner mới có quyền xóa).
    /// </summary>
    [HttpDelete("{roomId:guid}")]
    public async Task<IActionResult> DeleteRoom(Guid roomId)
    {
        var currentUserId = GetCurrentUserId();
        var member = await _roomRepository.GetMemberAsync(roomId, currentUserId);
        if (member == null || !member.IsOwner())
            return Forbid("Chỉ Chủ phòng mới có quyền xóa phòng.");

        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room != null)
        {
            await _roomRepository.DeleteAsync(room);
            await _unitOfWork.SaveChangesAsync();
        }

        return Ok(new { message = "Đã xóa phòng chat thành công." });
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

public record CreateRoomDto(string Name, int Type = 2, List<Guid>? InitialMemberIds = null);
public record AddMemberDto(Guid UserId);
public record TransferOwnerDto(Guid NewOwnerUserId);
