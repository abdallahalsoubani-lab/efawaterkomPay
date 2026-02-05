using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectPayGateway.Core.Entities;

public class Donation
{
    public int Id { get; set; }

    public int CampaignId { get; set; }

    [Required]
    [MaxLength(50)]
    public string JOEBPPSTrx { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? BankTrxId { get; set; }

    [MaxLength(10)]
    public string? BankCode { get; set; }

    [Required]
    [MaxLength(50)]
    public string BillingNo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? BillNo { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal DueAmount { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal PaidAmount { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal FeesAmount { get; set; }

    public bool FeesOnBiller { get; set; }

    [MaxLength(20)]
    public string PmtStatus { get; set; } = string.Empty;

    [MaxLength(3)]
    public string Currency { get; set; } = "JOD";

    [MaxLength(50)]
    public string? AccessChannel { get; set; }

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    [MaxLength(20)]
    public string? PaymentType { get; set; }

    public DateTime ProcessDate { get; set; }

    public DateTime StmtDate { get; set; }

    // Payer Info
    [MaxLength(10)]
    public string? PayerIdType { get; set; }

    [MaxLength(50)]
    public string? PayerId { get; set; }

    [MaxLength(5)]
    public string? PayerNation { get; set; }

    [MaxLength(200)]
    public string? PayerName { get; set; }

    [MaxLength(20)]
    public string? PayerPhone { get; set; }

    [MaxLength(250)]
    public string? PayerEmail { get; set; }

    // CTM Message Info
    [MaxLength(100)]
    public string? RequestGUID { get; set; }

    public string? RequestRaw { get; set; }

    public string? ResponseRaw { get; set; }

    // Acknowledgment tracking
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Campaign Campaign { get; set; } = null!;
}
