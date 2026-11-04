using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapping Room sang bảng Rooms, gồm loại phòng và user tạo phòng.
/// </summary>
public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        // Room.Id được sinh ngoài database để Domain giữ quyền tạo identity.
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(150);

        builder.Property(x => x.Type)
            // Enum RoomType lưu dạng int để database đơn giản và ổn định.
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .IsRequired();

        builder.HasIndex(x => x.CreatedByUserId);

        builder.HasIndex(x => x.Type);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            // Không xóa room chỉ vì user tạo phòng bị xóa, tránh mất lịch sử chat.
            .OnDelete(DeleteBehavior.Restrict);
    }
}
