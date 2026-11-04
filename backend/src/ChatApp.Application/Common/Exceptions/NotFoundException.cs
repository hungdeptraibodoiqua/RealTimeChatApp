using System;

namespace ChatApp.Application.Common.Exceptions
{
    /// <summary>
    /// Dùng cho
    /// User not found 
    /// Conversation not found
    /// Message not found
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, object key)
            : base($"{name} ({key}) was not found.")
        {
        }
    }
}

