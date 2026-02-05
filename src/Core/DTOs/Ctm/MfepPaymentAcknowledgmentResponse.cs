using System.Text.Json.Serialization;
using DirectPayGateway.Core.DTOs.Ctm.Shared;

namespace DirectPayGateway.Core.DTOs.Ctm;

public class MfepPaymentAcknowledgmentResponse
{
    [JsonPropertyName("MFEP")]
    public PaymentAcknowledgmentMfepResponse MFEP { get; set; } = new();
}

public class PaymentAcknowledgmentMfepResponse
{
    [JsonPropertyName("MsgHeader")]
    public MsgHeader MsgHeader { get; set; } = new();

    [JsonPropertyName("MsgBody")]
    public PaymentAcknowledgmentResponseBody MsgBody { get; set; } = new();
}

public class PaymentAcknowledgmentResponseBody
{
    [JsonPropertyName("Transactions")]
    public PaymentAcknowledgmentResponseTransactions Transactions { get; set; } = new();
}

public class PaymentAcknowledgmentResponseTransactions
{
    [JsonPropertyName("TrxInf")]
    public PaymentAcknowledgmentResponseTrxInf TrxInf { get; set; } = new();
}

public class PaymentAcknowledgmentResponseTrxInf
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
