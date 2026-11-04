using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapping User domain entity sang bảng Users và các ràng buộc đăng nhập duy nhất.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // Id do domain/application sinh, database không tự generate.
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            // Email là định danh đăng nhập nên phải duy nhất.
            .IsUnique();

        builder.HasIndex(x => x.Username)
            // Username là định danh đăng nhập nên phải duy nhất.
            .IsUnique();
    }
}
