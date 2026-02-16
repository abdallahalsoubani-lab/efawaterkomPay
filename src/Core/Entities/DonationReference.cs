using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectPayGateway.Core.Entities;

public class DonationReference
{
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string ReferenceNumber { get; set; } = string.Empty;

    public int CampaignId { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal? IntendedAmount { get; set; }

    [MaxLength(250)]
    public string? PayerEmail { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Used, Expired

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public int? DonationId { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;
    public virtual Donation? Donation { get; set; }
}
