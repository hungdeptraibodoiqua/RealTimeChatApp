namespace ChatApp.Application.Features.Auth.Commands.Login;

using ChatApp.Application.Abstractions.Persistence;
using ChatApp.Application.Abstractions.Security;
using ChatApp.Application.Common.Exceptions;
using ChatApp.Application.Common.Models;
using ChatApp.Application.Features.Auth.DTOs;
using ChatApp.Domain.Entities;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Use case đăng nhập: xác thực user, phát JWT và tạo refresh token đã hash trong database.
/// </summary>
public sealed class LoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly int _refreshTokenExpirationDays;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenExpirationDays = configuration.GetValue<int?>("Jwt:RefreshTokenExpirationDays") ?? 7;
    }

    public async Task<AuthResponseDto> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        // Email có ký tự @ thì tìm theo email, ngược lại tìm theo username.
        var identifier = command.EmailOrUserName.Trim();
        var user = identifier.Contains('@')
            ? await _userRepository.GetByEmailAsync(identifier, cancellationToken)
            : await _userRepository.GetByUsernameAsync(identifier, cancellationToken);

        if (user is null || !_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            throw new AppException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            throw new AppException("User is inactive.");
        }

        // jwtId liên kết access token vừa phát hành với refresh token lưu trong DB.
        var jwtId = _refreshTokenGenerator.GenerateJwtId();
        var currentUser = new CurrentUser
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(currentUser, jwtId);
        var accessTokenExpiresAtUtc = _jwtTokenGenerator.GetAccessTokenExpiresAtUtc();
        // Raw refresh token chỉ trả về client; database chỉ lưu hash để giảm rủi ro lộ token.
        var rawRefreshToken = _refreshTokenGenerator.GenerateToken();
        var tokenHash = _refreshTokenGenerator.HashToken(rawRefreshToken);

        var refreshToken = RefreshToken.Create(
            user.Id,
            tokenHash,
            DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            jwtId);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        // Commit user session mới sau khi refresh token đã được stage vào DbContext.
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

    private static void Validate(LoginCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(command.EmailOrUserName))
        {
            errors["EmailOrUserName"] = ["Email or username is required."];
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            errors["Password"] = ["Password is required."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}
