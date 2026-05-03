using System;

namespace ChatApp.Application.Common.Models
{
    /// <summary>
    /// Snapshot user đã xác thực dùng để phát JWT hoặc truyền identity vào Application service.
    /// </summary>
    public class CurrentUser
    {
        public Guid Id { get; init; }

        public string Username { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;
    }
}

