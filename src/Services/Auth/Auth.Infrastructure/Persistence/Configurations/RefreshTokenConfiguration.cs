namespace Auth.Infrastructure.Persistence.Configurations;

using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.UserId).HasColumnName("user_id");
        builder.Property(t => t.TokenHash).HasColumnName("token_hash").HasMaxLength(64);
        builder.Property(t => t.Jti).HasColumnName("jti").HasMaxLength(36);
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.Property(t => t.ExpiresAt).HasColumnName("expires_at");
        builder.Property(t => t.IsRevoked).HasColumnName("is_revoked");
        builder.Property(t => t.RevokedReason).HasColumnName("revoked_reason").HasMaxLength(100);
        builder.Property(t => t.RevokedAt).HasColumnName("revoked_at");

        builder.HasIndex(t => t.TokenHash);
        builder.HasIndex(t => new { t.UserId, t.IsRevoked });
    }
}
