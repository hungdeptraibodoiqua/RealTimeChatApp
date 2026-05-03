using Microsoft.EntityFrameworkCore;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;

namespace ChatApp.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext map các Domain entity sang PostgreSQL thông qua các configuration trong Infrastructure.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<RoomMember> RoomMembers => Set<RoomMember>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tự động áp dụng toàn bộ IEntityTypeConfiguration để mapping tập trung trong thư mục Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
