using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // JWT claim vẫn có thể là string trong payload, nhưng boundary API phải parse an toàn sang Guid trước khi đưa vào nghiệp vụ.
    protected static bool TryGetCurrentUserId(ClaimsPrincipal user, out Guid userId)
    {
        var rawUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(rawUserId, out userId);
    }
}
