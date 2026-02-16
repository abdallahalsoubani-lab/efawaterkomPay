using DirectPayGateway.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectPayGateway.Infrastructure.Data.Configurations;

public class DonationReferenceConfiguration : IEntityTypeConfiguration<DonationReference>
{
    public void Configure(EntityTypeBuilder<DonationReference> builder)
    {
        builder.HasKey(dr => dr.Id);

        builder.HasIndex(dr => dr.ReferenceNumber)
            .IsUnique();

        builder.HasIndex(dr => dr.CampaignId);
        builder.HasIndex(dr => dr.Status);
        builder.HasIndex(dr => dr.ExpiresAt);
        builder.HasIndex(dr => dr.DonationId);

        builder.Property(dr => dr.ReferenceNumber)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(dr => dr.IntendedAmount)
            .HasColumnType("decimal(18,3)");

        builder.Property(dr => dr.PayerEmail)
            .HasMaxLength(250);

        builder.Property(dr => dr.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(dr => dr.Campaign)
            .WithMany(c => c.DonationReferences)
            .HasForeignKey(dr => dr.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dr => dr.Donation)
            .WithMany()
            .HasForeignKey(dr => dr.DonationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
