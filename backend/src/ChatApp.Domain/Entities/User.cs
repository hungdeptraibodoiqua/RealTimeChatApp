using System;

namespace ChatApp.Domain.Entities;

/// <summary>
/// Đại diện tài khoản người dùng trong domain, gồm định danh đăng nhập và trạng thái hoạt động.
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string DisplayName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public User(Guid id, string username, string email, string passwordHash, string displayName, string? avatarUrl = null)
    {
        // Guid.Empty bị chặn để entity luôn có khóa hợp lệ trước khi lưu qua EF Core.
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash cannot be empty.", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("DisplayName cannot be empty.", nameof(displayName));

        Id = id;
        Username = username.Trim();
        Email = email.Trim();
        PasswordHash = passwordHash;
        DisplayName = displayName.Trim();
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    // Constructor rỗng chỉ dành cho EF Core materialize entity, không dùng cho business flow.
    private User()
    {
        Username = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        DisplayName = string.Empty;
    }

    /// <summary>
    /// Cập nhật tên hiển thị trong profile của user.
    /// </summary>
    public void UpdateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("DisplayName cannot be empty.", nameof(displayName));

        DisplayName = displayName.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash cannot be empty.", nameof(passwordHash));

        PasswordHash = passwordHash;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        Email = email.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.", nameof(username));

        Username = username.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateAvatarUrl(string? avatarUrl)
    {
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        // Soft deactivate giúp khóa tài khoản mà vẫn giữ dữ liệu liên quan như message/room membership.
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        // Kích hoạt lại tài khoản sau khi được phép sử dụng hệ thống.
        IsActive = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
