using System.ComponentModel.DataAnnotations;

namespace DirectPayGateway.Core.DTOs.Ctm;

public class CampaignDto
{
    public int Id { get; set; }
    public string CampaignCode { get; set; } = string.Empty;
    public string BillNo { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string BillType { get; set; } = string.Empty;
    public string BillCustomerCat { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public int DonorsCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool AllowPartialPayment { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public string IBAN { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string? CustName { get; set; }
    public string? FreeText { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public decimal ProgressPercentage =>
        TargetAmount > 0 ? Math.Round(CollectedAmount / TargetAmount * 100, 2) : 0;
}

public class CampaignDetailDto : CampaignDto
{
    public List<DonationDto> RecentDonations { get; set; } = new();
}

public class CreateCampaignRequest
{
    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Campaign code must be alphanumeric")]
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
    public string Status { get; set; } = "Active";

    [MaxLength(20)]
    public string BillType { get; set; } = "Recurring";

    [MaxLength(20)]
    public string BillCustomerCat { get; set; } = "Citizen";

    [Range(0, 999999999.999)]
    public decimal TargetAmount { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool AllowPartialPayment { get; set; } = true;

    [Range(0.001, 999999999.999)]
    public decimal MinAmount { get; set; } = 1.000m;

    [Range(0.001, 999999999.999)]
    public decimal MaxAmount { get; set; } = 100000.000m;

    [Required]
    [MaxLength(50)]
    public string IBAN { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string BankCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? CustName { get; set; }

    [MaxLength(500)]
    public string? FreeText { get; set; }

    [MaxLength(250)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }
}

public class UpdateCampaignRequest
{
    [MaxLength(200)]
    public string? NameAr { get; set; }

    [MaxLength(200)]
    public string? NameEn { get; set; }

    [MaxLength(1000)]
    public string? DescriptionAr { get; set; }

    [MaxLength(1000)]
    public string? DescriptionEn { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    [Range(0, 999999999.999)]
    public decimal? TargetAmount { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool? AllowPartialPayment { get; set; }

    [Range(0.001, 999999999.999)]
    public decimal? MinAmount { get; set; }

    [Range(0.001, 999999999.999)]
    public decimal? MaxAmount { get; set; }

    [MaxLength(50)]
    public string? IBAN { get; set; }

    [MaxLength(10)]
    public string? BankCode { get; set; }

    [MaxLength(200)]
    public string? CustName { get; set; }

    [MaxLength(500)]
    public string? FreeText { get; set; }

    [MaxLength(250)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }
}

public class CampaignFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? Category { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class DonationDto
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public string? CampaignCode { get; set; }
    public string JOEBPPSTrx { get; set; } = string.Empty;
    public string? BankTrxId { get; set; }
    public string? BankCode { get; set; }
    public string BillingNo { get; set; } = string.Empty;
    public decimal DueAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal FeesAmount { get; set; }
    public bool FeesOnBiller { get; set; }
    public string PmtStatus { get; set; } = string.Empty;
    public string Currency { get; set; } = "JOD";
    public string? AccessChannel { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentType { get; set; }
    public DateTime ProcessDate { get; set; }
    public DateTime StmtDate { get; set; }
    public string? PayerIdType { get; set; }
    public string? PayerId { get; set; }
    public string? PayerNation { get; set; }
    public string? PayerName { get; set; }
    public string? PayerPhone { get; set; }
    public string? PayerEmail { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DonationFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? CampaignId { get; set; }
    public string? JOEBPPSTrx { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? PmtStatus { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class CtmAuditLogDto
{
    public int Id { get; set; }
    public string ApiName { get; set; } = string.Empty;
    public string? GUID { get; set; }
    public string? RequestType { get; set; }
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public int ResponseCode { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
    public long DurationMs { get; set; }
}

public class CtmAuditLogFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? ApiName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? ResponseCode { get; set; }
    public string? SortBy { get; set; } = "Timestamp";
    public bool SortDescending { get; set; } = true;
}
