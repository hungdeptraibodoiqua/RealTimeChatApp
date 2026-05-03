using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapping Message sang bảng Messages, gồm sender, room, reply link và trạng thái soft delete.
/// </summary>
public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        // Message.Id do domain/application sinh để có thể tham chiếu trước khi commit nếu cần.
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.RoomId)
            .IsRequired();

        builder.Property(x => x.SenderUserId)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.MessageType)
            // MessageType lưu dạng int để database không phụ thuộc tên enum trong code.
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.EditedAtUtc);

        builder.Property(x => x.DeletedAtUtc);

        builder.HasIndex(x => new { x.RoomId, x.CreatedAtUtc });
        // Index trên RoomId + CreatedAtUtc hỗ trợ truy vấn timeline message theo phòng.

        builder.HasIndex(x => x.SenderUserId);

        builder.HasIndex(x => x.ReplyToMessageId);

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            // Xóa room thì message thuộc room đó cũng bị xóa theo.
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.SenderUserId)
            // Không xóa message chỉ vì user gửi bị xóa, để giữ lịch sử phòng.
            .OnDelete(DeleteBehavior.Restrict);

        // Reply link chỉ là tham chiếu tùy chọn tới message khác, nên dùng SetNull để không làm đứt history.
        builder.HasOne<Message>()
            .WithMany()
            .HasForeignKey(x => x.ReplyToMessageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
