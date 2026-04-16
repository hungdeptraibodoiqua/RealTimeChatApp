using ChatApp.Application.Common.Models;

namespace ChatApp.Application.Abstractions.Security;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    CurrentUser? User { get; }
}
