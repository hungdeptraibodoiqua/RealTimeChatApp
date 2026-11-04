using ChatApp.Application.Common.Models;

namespace ChatApp.Application.Abstractions.Security;

/// <summary>
/// Contract đọc user hiện tại từ request context mà Application có thể dùng không phụ thuộc HttpContext.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Id của user đã xác thực; null nếu request chưa đăng nhập.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Cho biết request hiện tại có identity hợp lệ hay không.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Snapshot thông tin user hiện tại dùng cho handler/service cần identity.
    /// </summary>
    CurrentUser? User { get; }
}
