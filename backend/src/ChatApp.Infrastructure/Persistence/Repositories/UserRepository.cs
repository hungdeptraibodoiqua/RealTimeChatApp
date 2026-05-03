using ChatApp.Application.Abstractions.Persistence;
using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation của IUserRepository; repository không tự gọi SaveChangesAsync.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            // Query chỉ đọc nên không cần EF change tracking.
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            // Dùng cho kiểm tra login/register, không sửa entity nên AsNoTracking là phù hợp.
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            // Dùng cho kiểm tra login/register, không sửa entity nên AsNoTracking là phù hợp.
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Username == username, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        // Repository chỉ stage entity vào DbContext; UnitOfWork sẽ commit.
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }
}
