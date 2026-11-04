using System;

namespace ChatApp.Application.Common.Models
{
    /// <summary>
    /// Base DTO cho response có định danh domain entity.
    /// </summary>
    public abstract class BaseDto
    {
        public Guid Id { get; init; }
    }
}

