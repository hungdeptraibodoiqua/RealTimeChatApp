using System.Security.Claims;
using ChatApp.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

/// <summary>
/// Controller cung cấp thông tin người dùng và API tìm kiếm người dùng.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Tìm kiếm người dùng theo từ khóa để gửi lời mời kết bạn hoặc mời vào nhóm chat.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new List<object>());

        var currentUserId = GetCurrentUserId();
        var users = await _userRepository.SearchUsersAsync(q, currentUserId);

        var dtos = users.Select(u => new
        {
            u.Id,
            u.Username,
            u.DisplayName,
            u.AvatarUrl
        });

        return Ok(dtos);
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
