using DirectPayGateway.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectPayGateway.Infrastructure.Data.Configurations;

public class DonationConfiguration : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        builder.HasKey(d => d.Id);

        builder.HasIndex(d => d.JOEBPPSTrx)
            .IsUnique();

        builder.HasIndex(d => d.BillingNo);

        builder.HasIndex(d => d.RequestGUID);

        builder.HasIndex(d => d.CampaignId);

        builder.HasIndex(d => d.ProcessDate);

        builder.Property(d => d.JOEBPPSTrx)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.BankTrxId)
            .HasMaxLength(50);

        builder.Property(d => d.BankCode)
            .HasMaxLength(10);

        builder.Property(d => d.BillingNo)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.BillNo)
            .HasMaxLength(50);

        builder.Property(d => d.DueAmount)
            .HasColumnType("decimal(18,3)");

        builder.Property(d => d.PaidAmount)
            .HasColumnType("decimal(18,3)");

        builder.Property(d => d.FeesAmount)
            .HasColumnType("decimal(18,3)");

        builder.Property(d => d.PmtStatus)
            .HasMaxLength(20);

        builder.Property(d => d.Currency)
            .HasMaxLength(3);

        builder.Property(d => d.AccessChannel)
            .HasMaxLength(50);

        builder.Property(d => d.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(d => d.PaymentType)
            .HasMaxLength(20);

        builder.Property(d => d.PayerIdType)
            .HasMaxLength(10);

        builder.Property(d => d.PayerId)
            .HasMaxLength(50);

        builder.Property(d => d.PayerNation)
            .HasMaxLength(5);

        builder.Property(d => d.PayerName)
            .HasMaxLength(200);

        builder.Property(d => d.PayerPhone)
            .HasMaxLength(20);

        builder.Property(d => d.PayerEmail)
            .HasMaxLength(250);

        builder.Property(d => d.RequestGUID)
            .HasMaxLength(100);

        builder.Property(d => d.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(d => d.Campaign)
            .WithMany(c => c.Donations)
            .HasForeignKey(d => d.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
