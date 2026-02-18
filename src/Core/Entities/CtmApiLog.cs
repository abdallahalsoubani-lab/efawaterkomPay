using System.ComponentModel.DataAnnotations;

namespace DirectPayGateway.Core.Entities;

public class CtmApiLog
{
    public int Id { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    public string Endpoint { get; set; } = string.Empty;

    [MaxLength(10)]
    public string HttpMethod { get; set; } = "POST";

    public string? RequestBody { get; set; }

    public string? ResponseBody { get; set; }

    public int HttpStatusCode { get; set; }

    [MaxLength(50)]
    public string? BillingNo { get; set; }

    [MaxLength(50)]
    public string? JOEBPPSTrx { get; set; }

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    [MaxLength(50)]
    public string? ClientIp { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public long ResponseTimeMs { get; set; }
}
