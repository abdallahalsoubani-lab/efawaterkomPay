using System.Text.Json.Serialization;
using DirectPayGateway.Core.DTOs.Ctm.Shared;

namespace DirectPayGateway.Core.DTOs.Ctm;

public class MfepBillPullRequest
{
    [JsonPropertyName("MFEP")]
    public BillPullMfepRequest MFEP { get; set; } = new();
}

public class BillPullMfepRequest
{
    [JsonPropertyName("MsgHeader")]
    public MsgHeader MsgHeader { get; set; } = new();

    [JsonPropertyName("MsgBody")]
    public BillPullRequestBody MsgBody { get; set; } = new();

    [JsonPropertyName("PayerInfo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PayerInfo? PayerInfo { get; set; }
}

public class BillPullRequestBody
{
    [JsonPropertyName("AcctInfo")]
    public AcctInfo AcctInfo { get; set; } = new();

    [JsonPropertyName("ServiceType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ServiceType { get; set; }
}
