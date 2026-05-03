namespace ChatApp.Application.Abstractions.Persistence;

/// <summary>
/// Ranh giới commit cho một use case; gom thay đổi từ nhiều repository thành một lần SaveChanges.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commit các thay đổi đang được DbContext track trong cùng transaction boundary.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
