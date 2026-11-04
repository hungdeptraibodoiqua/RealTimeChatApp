using System;

namespace ChatApp.Domain.Entities;

/// <summary>
/// Đại diện refresh token đã hash trong database, dùng cho login session và token rotation.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    // Chỉ lưu HASH của refresh token, raw token chỉ trả về client một lần.
    public string TokenHash { get; private set; }

    // Liên kết với access token/JWT đã phát hành để phục vụ audit hoặc revoke theo jti.
    public string JwtId { get; private set; }

    // Nhóm các refresh token cùng một chuỗi rotate để phát hiện reuse về sau.
    public string TokenFamily { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    public string? CreatedByIp { get; private set; }
    public string? RevokedByIp { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc; // Property với getter
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    // Constructor rỗng chỉ dành cho EF Core khi materialize token từ database.
    private RefreshToken()
    {
        TokenHash = string.Empty;
        JwtId = string.Empty;
        TokenFamily = string.Empty;
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        string tokenFamily,
        DateTime createdAtUtc,
        DateTime expiresAtUtc,
        string jwtId,
        Guid? replacedByTokenId = null,
        string? createdByIp = null)
    {
        // Guid.Empty bị chặn để token và user reference luôn có khóa hợp lệ.
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("TokenHash cannot be empty.", nameof(tokenHash));

        if (string.IsNullOrWhiteSpace(tokenFamily))
            throw new ArgumentException("TokenFamily cannot be empty.", nameof(tokenFamily));

        if (string.IsNullOrWhiteSpace(jwtId))
            throw new ArgumentException("JwtId cannot be empty.", nameof(jwtId));

        if (expiresAtUtc <= createdAtUtc)
            throw new ArgumentException("ExpiresAtUtc must be later than CreatedAtUtc.");

        Id = id;
        UserId = userId;
        ReplacedByTokenId = replacedByTokenId;
        TokenHash = tokenHash;
        TokenFamily = tokenFamily;
        JwtId = jwtId;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        CreatedByIp = createdByIp;
    }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc,
        string jwtId,
        string? createdByIp = null,
        string? tokenFamily = null,
        Guid? replacedByTokenId = null)
    {
        var now = DateTime.UtcNow;

        // UserId phải là user thật vì refresh token luôn thuộc về một account cụ thể.
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (expiresAtUtc <= now)
            throw new ArgumentException("ExpiresAtUtc must be in the future.", nameof(expiresAtUtc));

        return new RefreshToken(
            id: Guid.NewGuid(),
            userId: userId,
            tokenHash: tokenHash,
            tokenFamily: string.IsNullOrWhiteSpace(tokenFamily)
                // Nếu là token đầu tiên sau login thì mở family mới, nếu refresh thì giữ family cũ.
                ? Guid.NewGuid().ToString("N")
                : tokenFamily,
            createdAtUtc: now,
            expiresAtUtc: expiresAtUtc,
            jwtId: jwtId,
            replacedByTokenId: replacedByTokenId,
            createdByIp: createdByIp
        );
    }

    public void Revoke(string? revokedByIp, Guid? replacedByTokenId = null)
    {
        // Token đã revoke không được revoke lại để tránh che mất lịch sử rotate/reuse.
        if (IsRevoked)
            throw new InvalidOperationException("Refresh token has already been revoked.");

        RevokedAtUtc = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        // ReplacedByTokenId là tùy chọn, chỉ có khi token cũ được thay bằng token mới trong flow refresh.
        ReplacedByTokenId = replacedByTokenId;
    }

    /// <summary>
    /// Đảm bảo refresh token còn dùng được trước khi cấp access token mới.
    /// </summary>
    public void EnsureUsable()
    {
        if (IsRevoked)
            throw new InvalidOperationException("Refresh token has been revoked.");

        if (IsExpired)
            throw new InvalidOperationException("Refresh token has expired.");
    }
}
