using System.ComponentModel.DataAnnotations;
using DirectPayGateway.Core.Entities;

namespace DirectPayGateway.Core.DTOs.Payment;

public class PaymentInitiateRequest
{
    [Required]
    [Range(0.001, 999999999.999)]
    public decimal Amount { get; set; }

    [Required]
    [Range(1, 2)]
    public int PaymentType { get; set; } = 1; // 1=Postpaid, 2=Prepaid

    [MaxLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "BillingNo must be alphanumeric")]
    public string? BillingNo { get; set; }

    [Range(1, 99999)]
    public int? PrepaidCatCode { get; set; }

    [EmailAddress]
    [MaxLength(250)]
    public string? CustomerEmail { get; set; }

    [MaxLength(100)]
    [RegularExpression(@"^[^~""'&#%]*$", ErrorMessage = "Special characters not allowed: ~ \" ' & # %")]
    public string? StatementNarrative { get; set; }

    [MaxLength(250)]
    [RegularExpression(@"^[^~""'&#%]*$", ErrorMessage = "Special characters not allowed: ~ \" ' & # %")]
    public string? OtherDetails { get; set; }

    [Required]
    [RegularExpression(@"^(AR|EN)$", ErrorMessage = "Language must be AR or EN")]
    public string Language { get; set; } = "AR";
}

public class PaymentInitiateResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? TransactionId { get; set; }
    public string? BillerTrxNo { get; set; }
    public string? RedirectUrl { get; set; }
}

public class PaymentCallbackRequest
{
    public string ResponseParams { get; set; } = string.Empty;
}

public class PaymentCallbackResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? BillerTrxNo { get; set; }
    public int? TrxStatus { get; set; }
    public string? DirectPayTrxNo { get; set; }
    public decimal? Amount { get; set; }
    public int? PaymentStatus { get; set; }
    public string? OtherDetails { get; set; }
    public bool HashValid { get; set; }
}

public class TransactionDto
{
    public int Id { get; set; }
    public string BillerTrxNo { get; set; } = string.Empty;
    public string? DirectPayTrxNo { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? UserFullName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "JOD";
    public string? BillingNo { get; set; }
    public int PaymentType { get; set; }
    public string PaymentTypeName => PaymentType == 1 ? "Postpaid" : "Prepaid";
    public int? PrepaidCatCode { get; set; }
    public string? CustomerEmail { get; set; }
    public string? StatementNarrative { get; set; }
    public PaymentStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int? TrxStatusCode { get; set; }
    public string? TrxStatusMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class TransactionDetailDto : TransactionDto
{
    public string? OtherDetails { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? RequestUrl { get; set; }
    public string? ResponseRaw { get; set; }
    public List<TransactionLogDto> Logs { get; set; } = new();
}

public class TransactionLogDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}

public class TransactionFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? UserId { get; set; }
    public PaymentStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? BillerTrxNo { get; set; }
    public string? DirectPayTrxNo { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
