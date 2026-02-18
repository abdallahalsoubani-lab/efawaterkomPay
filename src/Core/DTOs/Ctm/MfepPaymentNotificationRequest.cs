using System.Text.Json.Serialization;
using DirectPayGateway.Core.DTOs.Ctm.Shared;

namespace DirectPayGateway.Core.DTOs.Ctm;

public class MfepPaymentNotificationRequest
{
    [JsonPropertyName("MFEP")]
    public PaymentNotificationMfepRequest MFEP { get; set; } = new();
}

public class PaymentNotificationMfepRequest
{
    [JsonPropertyName("MsgHeader")]
    public MsgHeader MsgHeader { get; set; } = new();

    [JsonPropertyName("MsgBody")]
    public PaymentNotificationRequestBody MsgBody { get; set; } = new();
}

public class PaymentNotificationRequestBody
{
    [JsonPropertyName("Transactions")]
    public PaymentNotificationTransactions Transactions { get; set; } = new();
}

public class PaymentNotificationTransactions
{
    [JsonPropertyName("TrxInf")]
    public PaymentNotificationTrxInf TrxInf { get; set; } = new();
}

public class PaymentNotificationTrxInf
{
    [JsonPropertyName("AcctInfo")]
    public AcctInfo AcctInfo { get; set; } = new();

    [JsonPropertyName("JOEBPPSTrx")]
    public string JOEBPPSTrx { get; set; } = string.Empty;

    [JsonPropertyName("BankTrxId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BankTrxId { get; set; }

    [JsonPropertyName("BankCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BankCode { get; set; }

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

    [JsonPropertyName("Currency")]
    public string Currency { get; set; } = "JOD";

    [JsonPropertyName("AccessChannel")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AccessChannel { get; set; }

    [JsonPropertyName("PaymentMethod")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("PaymentType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PaymentType { get; set; }

    [JsonPropertyName("ServiceTypeDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ServiceTypeDetails? ServiceTypeDetails { get; set; }

    [JsonPropertyName("PayerInfo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PayerInfo? PayerInfo { get; set; }
}
