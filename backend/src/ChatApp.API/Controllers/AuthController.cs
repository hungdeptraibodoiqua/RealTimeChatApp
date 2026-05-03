using System.Security.Claims;
using ChatApp.Application.Features.Auth.Commands.Login;
using ChatApp.Application.Features.Auth.Commands.RefreshToken;
using ChatApp.Application.Features.Auth.Commands.Register;
using ChatApp.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

/// <summary>
/// Controller xác thực: chỉ map HTTP request sang Application command, không chứa business logic.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RegisterCommandHandler _registerCommandHandler;
    private readonly LoginCommandHandler _loginCommandHandler;
    private readonly RefreshTokenCommandHandler _refreshTokenCommandHandler;

    public AuthController(
        RegisterCommandHandler registerCommandHandler,
        LoginCommandHandler loginCommandHandler,
        RefreshTokenCommandHandler refreshTokenCommandHandler)
    {
        _registerCommandHandler = registerCommandHandler;
        _loginCommandHandler = loginCommandHandler;
        _refreshTokenCommandHandler = refreshTokenCommandHandler;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponseDto>> Register(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        // Request từ POST /api/auth/register đi vào RegisterCommandHandler để tạo User.
        var response = await _registerCommandHandler.HandleAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        // Controller không verify password; LoginCommandHandler xử lý xác thực và phát token.
        var response = await _loginCommandHandler.HandleAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        // Refresh token flow được xử lý ở Application để rotate token và commit qua UnitOfWork.
        var response = await _refreshTokenCommandHandler.HandleAsync(command, cancellationToken);
        return Ok(response);
    }

    // JWT claim vẫn có thể là string trong payload, nhưng boundary API phải parse an toàn sang Guid trước khi đưa vào nghiệp vụ.
    protected static bool TryGetCurrentUserId(ClaimsPrincipal user, out Guid userId)
    {
        var rawUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(rawUserId, out userId);
    }
}
