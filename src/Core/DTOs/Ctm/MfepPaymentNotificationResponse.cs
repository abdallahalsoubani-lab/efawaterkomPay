using System.Text.Json.Serialization;
using DirectPayGateway.Core.DTOs.Ctm.Shared;

namespace DirectPayGateway.Core.DTOs.Ctm;

public class MfepPaymentNotificationResponse
{
    [JsonPropertyName("MFEP")]
    public PaymentNotificationMfepResponse MFEP { get; set; } = new();
}

public class PaymentNotificationMfepResponse
{
    [JsonPropertyName("MsgHeader")]
    public MsgHeader MsgHeader { get; set; } = new();

    [JsonPropertyName("MsgBody")]
    public PaymentNotificationResponseBody MsgBody { get; set; } = new();
}

public class PaymentNotificationResponseBody
{
    [JsonPropertyName("Transactions")]
    public PaymentNotificationResponseTransactions Transactions { get; set; } = new();
}

public class PaymentNotificationResponseTransactions
{
    [JsonPropertyName("TrxInf")]
    public PaymentNotificationResponseTrxInf TrxInf { get; set; } = new();
}

public class PaymentNotificationResponseTrxInf
{
    [JsonPropertyName("JOEBPPSTrx")]
    public string JOEBPPSTrx { get; set; } = string.Empty;

    [JsonPropertyName("ProcessDate")]
    public string ProcessDate { get; set; } = string.Empty;

    [JsonPropertyName("STMTDate")]
    public string STMTDate { get; set; } = string.Empty;

    [JsonPropertyName("Result")]
    public MfepResult Result { get; set; } = new();
}
