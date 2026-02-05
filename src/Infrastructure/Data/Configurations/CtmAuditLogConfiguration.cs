using DirectPayGateway.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectPayGateway.Infrastructure.Data.Configurations;

public class CtmAuditLogConfiguration : IEntityTypeConfiguration<CtmAuditLog>
{
    public void Configure(EntityTypeBuilder<CtmAuditLog> builder)
    {
        builder.HasKey(l => l.Id);

        builder.HasIndex(l => l.GUID);

        builder.HasIndex(l => l.ApiName);

        builder.HasIndex(l => l.Timestamp);

        builder.Property(l => l.ApiName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.GUID)
            .HasMaxLength(100);

        builder.Property(l => l.RequestType)
            .HasMaxLength(20);

        builder.Property(l => l.IpAddress)
            .HasMaxLength(50);

        builder.Property(l => l.Timestamp)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
