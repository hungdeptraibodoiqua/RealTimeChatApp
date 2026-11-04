namespace ChatApp.Application.Abstractions.Common
{
    /// <summary>
    /// Cung cấp cơ chế sinh ID theo cách trừu tượng hóa.
    /// Dùng để các handler/service sinh core business ID dưới dạng Guid nhất quán với Domain.
    /// </summary>
    public interface IIdGenerator
    {
        /// <summary>
        /// Sinh ra một ID mới cho entity nội bộ.
        /// </summary>
        Guid NewId();
    }
}
