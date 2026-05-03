namespace ChatApp.Application.Features.Auth.Commands.RefreshToken;

using ChatApp.Application.Abstractions.Persistence;
using ChatApp.Application.Abstractions.Security;
using ChatApp.Application.Common.Exceptions;
using ChatApp.Application.Common.Models;
using ChatApp.Application.Features.Auth.DTOs;
using ChatApp.Domain.Entities;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Use case refresh token: kiểm tra token cũ, rotate token mới và revoke token cũ.
/// </summary>
public sealed class RefreshTokenCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly int _refreshTokenExpirationDays;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenExpirationDays = configuration.GetValue<int?>("Jwt:RefreshTokenExpirationDays") ?? 7;
    }

    public async Task<AuthResponseDto> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        // Client gửi raw token, Application hash lại rồi tìm token hash đã lưu trong database.
        var tokenHash = _refreshTokenGenerator.HashToken(command.RefreshToken.Trim());
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new AppException("Refresh token is invalid.");

        try
        {
            // Domain entity tự kiểm tra trạng thái revoked/expired trước khi cấp token mới.
            existingToken.EnsureUsable();
        }
        catch (InvalidOperationException ex)
        {
            throw new AppException(ex.Message);
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), existingToken.UserId);

        if (!user.IsActive)
        {
            throw new AppException("User is inactive.");
        }

        // Mỗi lần refresh sẽ phát jti mới cho access token mới.
        var jwtId = _refreshTokenGenerator.GenerateJwtId();
        var currentUser = new CurrentUser
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(currentUser, jwtId);
        var accessTokenExpiresAtUtc = _jwtTokenGenerator.GetAccessTokenExpiresAtUtc();
        // Raw token mới chỉ trả về client, còn hash mới được lưu DB.
        var rawRefreshToken = _refreshTokenGenerator.GenerateToken();
        var newTokenHash = _refreshTokenGenerator.HashToken(rawRefreshToken);

        var newRefreshToken = RefreshToken.Create(
            user.Id,
            newTokenHash,
            DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            jwtId,
            // Giữ TokenFamily để toàn bộ chuỗi rotate vẫn truy vết được cùng một login session.
            tokenFamily: existingToken.TokenFamily,
            replacedByTokenId: existingToken.Id);

        // Revoke token cũ và liên kết tới token mới để tránh reuse token đã dùng.
        existingToken.Revoke(null, newRefreshToken.Id);
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        // Commit revoke token cũ và thêm token mới trong cùng một transaction boundary.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Tokens = new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                ExpiresAtUtc = accessTokenExpiresAtUtc,
                JwtId = jwtId
            }
        };
    }

    private static void Validate(RefreshTokenCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["RefreshToken"] = ["Refresh token is required."]
            });
        }
    }
}
