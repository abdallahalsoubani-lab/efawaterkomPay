using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectPayGateway.Core.Entities;

public class Campaign
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string CampaignCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string BillNo { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string NameEn { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? DescriptionAr { get; set; }

    [MaxLength(1000)]
    public string? DescriptionEn { get; set; }

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ServiceType { get; set; } = "Donations";

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Active"; // Active, Upcoming, Closed

    [MaxLength(20)]
    public string BillType { get; set; } = "Recurring"; // Recurring, OneTime

    [MaxLength(20)]
    public string BillCustomerCat { get; set; } = "Citizen"; // Citizen, Foreign

    [Column(TypeName = "decimal(18,3)")]
    public decimal TargetAmount { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal CollectedAmount { get; set; }

    public int DonorsCount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    // Payment Constraints
    public bool AllowPartialPayment { get; set; } = true;

    [Column(TypeName = "decimal(18,3)")]
    public decimal MinAmount { get; set; } = 1.000m;

    [Column(TypeName = "decimal(18,3)")]
    public decimal MaxAmount { get; set; } = 100000.000m;

    // Bank Account
    [MaxLength(50)]
    public string IBAN { get; set; } = string.Empty;

    [MaxLength(10)]
    public string BankCode { get; set; } = string.Empty;

    // Additional Info
    [MaxLength(200)]
    public string? CustName { get; set; }

    [MaxLength(500)]
    public string? FreeText { get; set; }

    [MaxLength(250)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
