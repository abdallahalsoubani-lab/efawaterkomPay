using DirectPayGateway.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectPayGateway.Infrastructure.Data.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.CampaignCode)
            .IsUnique();

        builder.HasIndex(c => c.Status);

        builder.Property(c => c.CampaignCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.BillNo)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.NameAr)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.NameEn)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.DescriptionAr)
            .HasMaxLength(1000);

        builder.Property(c => c.DescriptionEn)
            .HasMaxLength(1000);

        builder.Property(c => c.Category)
            .HasMaxLength(100);

        builder.Property(c => c.ServiceType)
            .HasMaxLength(50);

        builder.Property(c => c.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.BillType)
            .HasMaxLength(20);

        builder.Property(c => c.BillCustomerCat)
            .HasMaxLength(20);

        builder.Property(c => c.TargetAmount)
            .HasColumnType("decimal(18,3)");

        builder.Property(c => c.CollectedAmount)
            .HasColumnType("decimal(18,3)");

        builder.Property(c => c.MinAmount)
            .HasColumnType("decimal(18,3)");

        builder.Property(c => c.MaxAmount)
            .HasColumnType("decimal(18,3)");

        builder.Property(c => c.IBAN)
            .HasMaxLength(50);

        builder.Property(c => c.BankCode)
            .HasMaxLength(10);

        builder.Property(c => c.CustName)
            .HasMaxLength(200);

        builder.Property(c => c.FreeText)
            .HasMaxLength(500);

        builder.Property(c => c.Email)
            .HasMaxLength(250);

        builder.Property(c => c.Phone)
            .HasMaxLength(20);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasMany(c => c.Donations)
            .WithOne(d => d.Campaign)
            .HasForeignKey(d => d.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
