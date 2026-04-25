using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Persistence.Configurations;

public sealed class RoomMemberConfiguration : IEntityTypeConfiguration<RoomMember>
{
    public void Configure(EntityTypeBuilder<RoomMember> builder)
    {
        builder.ToTable("RoomMembers");

        builder.HasKey(x => new { x.RoomId, x.UserId });

        builder.Property(x => x.Role)
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
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // LastReadMessageId chỉ ghi nhận tiến độ đọc, message bị xóa thì reset về null.
        builder.HasOne<ChatApp.Domain.Enums.Message>()
            .WithMany()
            .HasForeignKey(x => x.LastReadMessageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
