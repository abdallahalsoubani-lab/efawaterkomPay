using System.ComponentModel.DataAnnotations;

namespace DirectPayGateway.Core.Entities;

public class TransactionLog
{
    public int Id { get; set; }

    public int PaymentTransactionId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    public string? Details { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(100)]
    public string? UserAgent { get; set; }

    public virtual PaymentTransaction PaymentTransaction { get; set; } = null!;
}
