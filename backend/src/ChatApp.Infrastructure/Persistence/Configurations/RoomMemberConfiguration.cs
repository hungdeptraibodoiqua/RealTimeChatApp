using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapping RoomMember, quan hệ nhiều-nhiều giữa User và Room có thêm role/trạng thái đọc.
/// </summary>
public sealed class RoomMemberConfiguration : IEntityTypeConfiguration<RoomMember>
{
    public void Configure(EntityTypeBuilder<RoomMember> builder)
    {
        builder.ToTable("RoomMembers");

        // Một user chỉ có một membership record trong cùng một room.
        builder.HasKey(x => new { x.RoomId, x.UserId });

        builder.Property(x => x.Role)
            // MemberRole lưu dạng int để mapping enum đơn giản.
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.JoinedAtUtc)
            .IsRequired();

        builder.Property(x => x.LeftAtUtc);

        builder.Property(x => x.LastReadMessageId);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.LastReadMessageId);

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            // Xóa room thì membership không còn ý nghĩa nên cascade là phù hợp.
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            // Xóa user thì gỡ membership của user khỏi các room.
            .OnDelete(DeleteBehavior.Cascade);

        // LastReadMessageId chỉ ghi nhận tiến độ đọc, message bị xóa thì reset về null.
        builder.HasOne<Message>()
            .WithMany()
            .HasForeignKey(x => x.LastReadMessageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
