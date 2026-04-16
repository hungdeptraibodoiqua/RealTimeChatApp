using System;

namespace ChatApp.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    // Chỉ lưu HASH của refresh token, không lưu raw token
    public string TokenHash { get; private set; }

    // Liên kết với access token/JWT đã phát hành (nếu bạn muốn quản lý chặt hơn)
    public string? JwtId { get; private set; }

    // Dùng để nhóm các token cùng 1 "family" khi rotate
    public string TokenFamily { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    public string? CreatedByIp { get; private set; }
    public string? RevokedByIp { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string? ReasonRevoked { get; private set; }

    // Metadata thêm cho session/device
    public string? DeviceName { get; private set; }
    public string? UserAgent { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc; // Property với getter
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    ////parameterless constructor (constructor rỗng) => không dùng cho business logic, dùng cho Entity Framework Core.
    ////Persistence Ignorance + Encapsulation pattern
    private RefreshToken()
    {
        TokenHash = string.Empty;
        TokenFamily = string.Empty;
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        string tokenFamily,
        DateTime createdAtUtc,
        DateTime expiresAtUtc,
        string? jwtId = null,
        string? createdByIp = null,
        string? deviceName = null,
        string? userAgent = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("TokenHash cannot be empty.", nameof(tokenHash));

        if (string.IsNullOrWhiteSpace(tokenFamily))
            throw new ArgumentException("TokenFamily cannot be empty.", nameof(tokenFamily));

        if (expiresAtUtc <= createdAtUtc)
            throw new ArgumentException("ExpiresAtUtc must be later than CreatedAtUtc.");

        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        TokenFamily = tokenFamily;
        JwtId = jwtId;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        CreatedByIp = createdByIp;
        DeviceName = deviceName;
        UserAgent = userAgent;
    }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc,
        string? jwtId = null,
        string? createdByIp = null,
        string? deviceName = null,
        string? userAgent = null,
        string? tokenFamily = null)
    {
        var now = DateTime.UtcNow;

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (expiresAtUtc <= now)
            throw new ArgumentException("ExpiresAtUtc must be in the future.", nameof(expiresAtUtc));

        return new RefreshToken(
            id: Guid.NewGuid(),
            userId: userId,
            tokenHash: tokenHash,
            tokenFamily: string.IsNullOrWhiteSpace(tokenFamily)
                ? Guid.NewGuid().ToString("N")
                : tokenFamily,
            createdAtUtc: now,
            expiresAtUtc: expiresAtUtc,
            jwtId: jwtId,
            createdByIp: createdByIp,
            deviceName: deviceName,
            userAgent: userAgent
        );
    }

    public void Revoke(string? revokedByIp, string? reason = null, string? replacedByTokenHash = null)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Refresh token has already been revoked.");

        RevokedAtUtc = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReasonRevoked = reason;
        ReplacedByTokenHash = replacedByTokenHash;
    }

    public void EnsureUsable()
    {
        if (IsRevoked)
            throw new InvalidOperationException("Refresh token has been revoked.");

        if (IsExpired)
            throw new InvalidOperationException("Refresh token has expired.");
    }
}
