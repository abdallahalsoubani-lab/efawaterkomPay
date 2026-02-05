using System.Text.Json.Serialization;
using DirectPayGateway.Core.DTOs.Ctm.Shared;

namespace DirectPayGateway.Core.DTOs.Ctm;

public class MfepPaymentAcknowledgmentRequest
{
    [JsonPropertyName("MFEP")]
    public PaymentAcknowledgmentMfepRequest MFEP { get; set; } = new();
}

public class PaymentAcknowledgmentMfepRequest
{
    [JsonPropertyName("MsgHeader")]
    public MsgHeader MsgHeader { get; set; } = new();

    [JsonPropertyName("MsgBody")]
    public PaymentAcknowledgmentRequestBody MsgBody { get; set; } = new();
}

public class PaymentAcknowledgmentRequestBody
{
    [JsonPropertyName("BillingInfo")]
    public BillingInfo BillingInfo { get; set; } = new();
}

public class BillingInfo
{
    [JsonPropertyName("AcctInfo")]
    public AcctInfo AcctInfo { get; set; } = new();

    [JsonPropertyName("JOEBPPSTrx")]
    public string JOEBPPSTrx { get; set; } = string.Empty;

    [JsonPropertyName("ParTrxId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ParTrxId { get; set; }

    [JsonPropertyName("PmtSrc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PmtSrc { get; set; }

    [JsonPropertyName("PmtStatus")]
    public string PmtStatus { get; set; } = string.Empty;

    [JsonPropertyName("DueAmt")]
    public string DueAmt { get; set; } = "0.000";

    [JsonPropertyName("PaidAmt")]
    public string PaidAmt { get; set; } = "0.000";

    [JsonPropertyName("FeesAmt")]
    public string FeesAmt { get; set; } = "0.000";

    [JsonPropertyName("FeesOnBiller")]
    public bool FeesOnBiller { get; set; }

    [JsonPropertyName("ProcessDate")]
    public string ProcessDate { get; set; } = string.Empty;

    [JsonPropertyName("StmtDate")]
    public string StmtDate { get; set; } = string.Empty;

    [JsonPropertyName("AccessChannel")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AccessChannel { get; set; }

    [JsonPropertyName("PaymentMethod")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("PaymentType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PaymentType { get; set; }

    [JsonPropertyName("PspCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PspCode { get; set; }

    [JsonPropertyName("ParFees")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? ParFees { get; set; }

    [JsonPropertyName("ServiceTypeDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ServiceTypeDetails? ServiceTypeDetails { get; set; }

    [JsonPropertyName("AdditionalInfo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AdditionalInfo? AdditionalInfo { get; set; }
}
