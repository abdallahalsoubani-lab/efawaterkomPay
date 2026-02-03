using DirectPayGateway.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectPayGateway.Infrastructure.Data;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.Token)
            .IsUnique();

        builder.HasIndex(r => r.UserId);

        builder.Property(r => r.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.CreatedByIp)
            .HasMaxLength(50);

        builder.Property(r => r.RevokedByIp)
            .HasMaxLength(50);

        builder.Property(r => r.ReplacedByToken)
            .HasMaxLength(500);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
