namespace ChatApp.Application.Abstractions.Common
{
    /// <summary>
    /// Cung cấp cơ chế sinh ID theo cách trừu tượng hóa.
    /// Dùng để tránh phụ thuộc trực tiếp Guid.NewGuid() trong handler/service,
    /// đồng thời dễ thay đổi chiến lược sinh ID về sau.
    /// </summary>
    public interface IIdGenerator
    {
        /// <summary>
        /// Sinh ra một ID mới dạng chuỗi.
        /// Hiện phù hợp với domain đang dùng nhiều string id.
        /// </summary>
        string NewId();
    }
}