using DirectPayGateway.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectPayGateway.Infrastructure.Data;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasIndex(p => p.BillerTrxNo)
            .IsUnique();

        builder.HasIndex(p => p.DirectPayTrxNo);

        builder.HasIndex(p => p.UserId);

        builder.HasIndex(p => p.Status);

        builder.HasIndex(p => p.CreatedAt);

        builder.Property(p => p.BillerTrxNo)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.DirectPayTrxNo)
            .HasMaxLength(50);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,3)")
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("JOD")
            .IsRequired();

        builder.Property(p => p.BillingNo)
            .HasMaxLength(50);

        builder.Property(p => p.CustomerEmail)
            .HasMaxLength(250);

        builder.Property(p => p.StatementNarrative)
            .HasMaxLength(100);

        builder.Property(p => p.OtherDetails)
            .HasMaxLength(250);

        builder.Property(p => p.TrxStatusMessage)
            .HasMaxLength(500);

        builder.Property(p => p.RequestHash)
            .HasMaxLength(500);

        builder.Property(p => p.ClientIpAddress)
            .HasMaxLength(50);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(p => p.Logs)
            .WithOne(l => l.PaymentTransaction)
            .HasForeignKey(l => l.PaymentTransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
