using DirectPayGateway.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectPayGateway.Infrastructure.Data.Configurations;

public class CtmApiLogConfiguration : IEntityTypeConfiguration<CtmApiLog>
{
    public void Configure(EntityTypeBuilder<CtmApiLog> builder)
    {
        builder.HasKey(l => l.Id);

        builder.HasIndex(l => l.Timestamp);

        builder.HasIndex(l => l.Endpoint);

        builder.HasIndex(l => l.JOEBPPSTrx);

        builder.HasIndex(l => l.BillingNo);

        builder.Property(l => l.Endpoint)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.HttpMethod)
            .HasMaxLength(10);

        builder.Property(l => l.BillingNo)
            .HasMaxLength(50);

        builder.Property(l => l.JOEBPPSTrx)
            .HasMaxLength(50);

        builder.Property(l => l.ErrorMessage)
            .HasMaxLength(500);

        builder.Property(l => l.ClientIp)
            .HasMaxLength(50);

        builder.Property(l => l.UserAgent)
            .HasMaxLength(500);

        builder.Property(l => l.Timestamp)
            .HasDefaultValueSql("datetime('now')");
    }
}
