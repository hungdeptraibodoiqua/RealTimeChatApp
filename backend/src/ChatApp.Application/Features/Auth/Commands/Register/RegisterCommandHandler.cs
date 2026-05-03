namespace ChatApp.Application.Features.Auth.Commands.Register;

using ChatApp.Application.Abstractions.Persistence;
using ChatApp.Application.Abstractions.Security;
using ChatApp.Application.Common.Exceptions;
using ChatApp.Application.Features.Auth.DTOs;
using ChatApp.Domain.Entities;

/// <summary>
/// Use case đăng ký user mới: kiểm tra trùng, hash password, tạo User và commit qua UnitOfWork.
/// </summary>
public sealed class RegisterCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto> HandleAsync(
        RegisterCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        // Chuẩn hóa email/username trước khi kiểm tra trùng để dữ liệu đăng nhập nhất quán.
        var normalizedEmail = command.Email.Trim();
        var normalizedUsername = command.Username.Trim();

        if (await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            throw new AppException("Email already exists.");
        }

        if (await _userRepository.GetByUsernameAsync(normalizedUsername, cancellationToken) is not null)
        {
            throw new AppException("Username already exists.");
        }

        // Application chỉ nhận password hash từ abstraction, implementation BCrypt nằm ở Infrastructure.
        var passwordHash = _passwordHasher.HashPassword(command.Password);

        var user = new User(
            Guid.NewGuid(),
            normalizedUsername,
            normalizedEmail,
            passwordHash,
            command.DisplayName,
            command.AvatarUrl);

        await _userRepository.AddAsync(user, cancellationToken);
        // Commit user mới tại một transaction boundary rõ ràng thay vì commit trong repository.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName
        };
    }

    private static void Validate(RegisterCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(command.Username))
        {
            errors["Username"] = ["Username is required."];
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            errors["Email"] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            errors["Password"] = ["Password is required."];
        }

        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            errors["DisplayName"] = ["DisplayName is required."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}
