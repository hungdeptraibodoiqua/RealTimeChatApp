using System;

namespace ChatApp.Application.Common.Models
{
    public class CurrentUser
    {
        public Guid Id { get; init; }

        public string Username { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;
    }
}

