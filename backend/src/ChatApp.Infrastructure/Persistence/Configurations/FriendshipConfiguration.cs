using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình mapping EF Core cho entity Friendship sang bảng Friendships trong cơ sở dữ liệu.
/// Kết nối với: Friendship.cs (Domain entity) và AppDbContext.cs.
/// </summary>
public sealed class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("Friendships");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.RequesterId).IsRequired();
        builder.Property(x => x.AddresseeId).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        // Index tìm kiếm theo từng User
        builder.HasIndex(x => x.RequesterId);
        builder.HasIndex(x => x.AddresseeId);
        builder.HasIndex(x => new { x.RequesterId, x.AddresseeId }).IsUnique();

        // Liên kết tới User
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.RequesterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AddresseeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
