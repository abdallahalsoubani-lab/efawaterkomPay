using DirectPayGateway.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectPayGateway.Infrastructure.Data;

public class TransactionLogConfiguration : IEntityTypeConfiguration<TransactionLog>
{
    public void Configure(EntityTypeBuilder<TransactionLog> builder)
    {
        builder.HasKey(l => l.Id);

        builder.HasIndex(l => l.PaymentTransactionId);

        builder.HasIndex(l => l.Timestamp);

        builder.Property(l => l.Action)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.IpAddress)
            .HasMaxLength(50);

        builder.Property(l => l.UserAgent)
            .HasMaxLength(100);

        builder.Property(l => l.Timestamp)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
