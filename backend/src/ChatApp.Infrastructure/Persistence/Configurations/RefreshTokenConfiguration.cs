using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapping RefreshToken đã hash, phục vụ login session và token rotation.
/// </summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        // RefreshToken.Id do domain tạo để token cũ có thể trỏ tới token thay thế.
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ReplacedByTokenId);

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.JwtId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.TokenFamily)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedByIp)
            .HasMaxLength(64);

        builder.Property(x => x.RevokedByIp)
            .HasMaxLength(64);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.ReplacedByTokenId);

        builder.HasIndex(x => x.JwtId);

        builder.HasIndex(x => x.TokenFamily);

        builder.HasIndex(x => x.ExpiresAtUtc);

        builder.HasIndex(x => x.TokenHash)
            // TokenHash phải duy nhất để một raw token chỉ khớp một session.
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            // Xóa user thì các refresh token của user không còn dùng được.
            .OnDelete(DeleteBehavior.Cascade);
    }
}
