using System;
using System.Collections.Generic;

namespace ChatApp.Application.Common.Exceptions
{
    /// <summary>
    /// Lưu exception dưới dạng
    /// {
    /// "Email": ["Email không hợp lệ", "Email đã tồn tại"],
    /// "Password": ["Mật khẩu quá ngắn"]
    /// }
    /// </summary>
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(
            IDictionary<string, string[]> errors)
        {
            Errors = errors;
        }
    }
}

