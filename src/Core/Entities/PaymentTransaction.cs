using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectPayGateway.Core.Entities;

public class PaymentTransaction
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string BillerTrxNo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? DirectPayTrxNo { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,3)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "JOD";

    [MaxLength(50)]
    public string? BillingNo { get; set; }

    public int PaymentType { get; set; } = 1; // 1=Postpaid, 2=Prepaid

    public int? PrepaidCatCode { get; set; }

    [MaxLength(250)]
    public string? CustomerEmail { get; set; }

    [MaxLength(100)]
    public string? StatementNarrative { get; set; }

    [MaxLength(250)]
    public string? OtherDetails { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public int? TrxStatusCode { get; set; }

    [MaxLength(500)]
    public string? TrxStatusMessage { get; set; }

    public int? DirectPayPaymentStatus { get; set; } // 1=Success, 2=Under Processing, 3=Failed

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [MaxLength(500)]
    public string? RequestHash { get; set; }

    public string? RequestUrl { get; set; }

    public string? ResponseRaw { get; set; }

    [MaxLength(50)]
    public string? ClientIpAddress { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<TransactionLog> Logs { get; set; } = new List<TransactionLog>();
}

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Success = 2,
    Failed = 3,
    Cancelled = 4
}
