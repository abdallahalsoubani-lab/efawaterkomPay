using System.ComponentModel.DataAnnotations;

namespace DirectPayGateway.Core.Entities;

public class CtmAuditLog
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ApiName { get; set; } = string.Empty; // BillPull, PaymentNotification, PaymentAcknowledgment

    [MaxLength(100)]
    public string? GUID { get; set; }

    [MaxLength(20)]
    public string? RequestType { get; set; } // BILPULRQ, BLRPMTNTFRQ, PMTACKRQ

    public string? RequestBody { get; set; }

    public string? ResponseBody { get; set; }

    public int ResponseCode { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public long DurationMs { get; set; }
}
