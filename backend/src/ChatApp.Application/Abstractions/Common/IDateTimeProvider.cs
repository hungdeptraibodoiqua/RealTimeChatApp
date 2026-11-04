using System;

namespace ChatApp.Application.Abstractions.Common
{
    /// <summary>
    /// Cung cấp thời gian hệ thống theo cách trừu tượng hóa.
    /// Dùng để tránh gọi trực tiếp DateTime.UtcNow trong Application/Infrastructure,
    /// giúp dễ test và đồng nhất nguồn thời gian.
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Lấy thời gian UTC hiện tại.
        /// </summary>
        DateTime UtcNow { get; }
    }
}