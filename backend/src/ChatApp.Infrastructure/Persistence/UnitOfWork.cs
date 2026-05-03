using ChatApp.Application.Abstractions.Persistence;

namespace ChatApp.Infrastructure.Persistence;

/// <summary>
/// Implementation UnitOfWork dùng AppDbContext để commit thay đổi của một use case.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Đây là điểm duy nhất commit thay đổi từ repository xuống database trong một request/use case.
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
