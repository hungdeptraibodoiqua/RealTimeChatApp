using System.Security.Claims;
using ChatApp.Application.Abstractions.Persistence;
using ChatApp.Application.Abstractions.Realtime;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

/// <summary>
/// Controller quản lý tính năng kết bạn: gửi lời mời, chấp nhận, từ chối và lấy danh sách bạn bè.
/// Kết nối với: FriendshipRepository, UserRepository, IChatNotifier, AppDbContext/UnitOfWork.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatNotifier _chatNotifier;

    public FriendsController(
        IFriendshipRepository friendshipRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IChatNotifier chatNotifier)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _chatNotifier = chatNotifier;
    }

    /// <summary>
    /// Lấy danh sách bạn bè đã được chấp nhận của user hiện tại.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyFriends()
    {
        var currentUserId = GetCurrentUserId();
        var friendships = await _friendshipRepository.GetUserFriendsAsync(currentUserId);

        var friendDtos = new List<object>();
        foreach (var f in friendships)
        {
            var otherUserId = f.RequesterId == currentUserId ? f.AddresseeId : f.RequesterId;
            var friendUser = await _userRepository.GetByIdAsync(otherUserId);
            if (friendUser != null)
            {
                friendDtos.Add(new
                {
                    f.Id,
                    FriendUserId = friendUser.Id,
                    friendUser.Username,
                    friendUser.DisplayName,
                    friendUser.AvatarUrl,
                    f.CreatedAtUtc
                });
            }
        }

        return Ok(friendDtos);
    }

    /// <summary>
    /// Lấy danh sách các lời mời kết bạn đang chờ user hiện tại phản hồi.
    /// </summary>
    [HttpGet("requests")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var currentUserId = GetCurrentUserId();
        var requests = await _friendshipRepository.GetPendingRequestsAsync(currentUserId);

        var dtos = new List<object>();
        foreach (var req in requests)
        {
            var sender = await _userRepository.GetByIdAsync(req.RequesterId);
            if (sender != null)
            {
                dtos.Add(new
                {
                    req.Id,
                    req.RequesterId,
                    sender.Username,
                    sender.DisplayName,
                    sender.AvatarUrl,
                    req.CreatedAtUtc
                });
            }
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Gửi lời mời kết bạn tới một user khác.
    /// </summary>
    [HttpPost("request")]
    public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestDto request)
    {
        var currentUserId = GetCurrentUserId();
        if (request.TargetUserId == currentUserId)
            return BadRequest(new { detail = "Không thể gửi lời mời kết bạn cho chính mình." });

        var targetUser = await _userRepository.GetByIdAsync(request.TargetUserId);
        if (targetUser == null)
            return NotFound(new { detail = "Người dùng không tồn tại." });

        var existing = await _friendshipRepository.GetFriendshipAsync(currentUserId, request.TargetUserId);
        if (existing != null)
        {
            if (existing.Status == FriendshipStatus.Accepted)
                return BadRequest(new { detail = "Hai người đã là bạn bè của nhau." });

            if (existing.Status == FriendshipStatus.Pending)
                return BadRequest(new { detail = "Lời mời kết bạn đã được gửi trước đó và đang chờ phản hồi." });
        }

        var friendship = new Friendship(Guid.NewGuid(), currentUserId, request.TargetUserId);
        await _friendshipRepository.AddAsync(friendship);
        await _unitOfWork.SaveChangesAsync();

        var currentUser = await _userRepository.GetByIdAsync(currentUserId);

        // Báo realtime tới người nhận lời mời
        await _chatNotifier.FriendRequestReceivedAsync(request.TargetUserId, new
        {
            friendship.Id,
            friendship.RequesterId,
            RequesterDisplayName = currentUser?.DisplayName ?? "Người dùng",
            RequesterUsername = currentUser?.Username ?? "user",
            friendship.CreatedAtUtc
        });

        return Ok(new { message = "Đã gửi lời mời kết bạn thành công.", friendshipId = friendship.Id });
    }

    /// <summary>
    /// Chấp nhận lời mời kết bạn.
    /// </summary>
    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> AcceptFriendRequest(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var friendship = await _friendshipRepository.GetByIdAsync(id);
        if (friendship == null)
            return NotFound(new { detail = "Lời mời kết bạn không tồn tại." });

        if (friendship.AddresseeId != currentUserId)
            return Forbid("Bạn không có quyền chấp nhận lời mời này.");

        try
        {
            friendship.Accept();
            await _unitOfWork.SaveChangesAsync();

            var currentUser = await _userRepository.GetByIdAsync(currentUserId);

            // Báo cho người gửi biết lời mời đã được chấp nhận
            await _chatNotifier.FriendRequestAcceptedAsync(friendship.RequesterId, new
            {
                friendship.Id,
                FriendUserId = currentUserId,
                DisplayName = currentUser?.DisplayName,
                Username = currentUser?.Username
            });

            return Ok(new { message = "Đã chấp nhận lời mời kết bạn." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
    }

    /// <summary>
    /// Từ chối lời mời kết bạn.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectFriendRequest(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var friendship = await _friendshipRepository.GetByIdAsync(id);
        if (friendship == null)
            return NotFound(new { detail = "Lời mời kết bạn không tồn tại." });

        if (friendship.AddresseeId != currentUserId)
            return Forbid("Bạn không có quyền từ chối lời mời này.");

        try
        {
            friendship.Reject();
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { message = "Đã từ chối lời mời kết bạn." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

public record SendFriendRequestDto(Guid TargetUserId);
