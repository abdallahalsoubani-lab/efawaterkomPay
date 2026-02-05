using System.Text.Json.Serialization;
using DirectPayGateway.Core.DTOs.Ctm.Shared;

namespace DirectPayGateway.Core.DTOs.Ctm;

public class MfepBillPullResponse
{
    [JsonPropertyName("MFEP")]
    public BillPullMfepResponse MFEP { get; set; } = new();
}

public class BillPullMfepResponse
{
    [JsonPropertyName("MsgHeader")]
    public MsgHeader MsgHeader { get; set; } = new();

    [JsonPropertyName("MsgBody")]
    public BillPullResponseBody MsgBody { get; set; } = new();
}

public class BillPullResponseBody
{
    [JsonPropertyName("RecCount")]
    public int RecCount { get; set; }

    [JsonPropertyName("BillRec")]
    public List<BillRecord> BillRec { get; set; } = new();
}

public class BillRecord
{
    [JsonPropertyName("Result")]
    public MfepResult Result { get; set; } = new();

    [JsonPropertyName("AcctInfo")]
    public AcctInfo AcctInfo { get; set; } = new();

    [JsonPropertyName("BillStatus")]
    public string BillStatus { get; set; } = string.Empty;

    [JsonPropertyName("DueAmount")]
    public string DueAmount { get; set; } = "0.000";

    [JsonPropertyName("IssueDate")]
    public string IssueDate { get; set; } = string.Empty;

    [JsonPropertyName("DueDate")]
    public string DueDate { get; set; } = string.Empty;

    [JsonPropertyName("CloseDate")]
    public string CloseDate { get; set; } = string.Empty;

    [JsonPropertyName("ServiceType")]
    public string ServiceType { get; set; } = string.Empty;

    [JsonPropertyName("BillType")]
    public string BillType { get; set; } = string.Empty;

    [JsonPropertyName("PmtConst")]
    public PmtConst PmtConst { get; set; } = new();

    [JsonPropertyName("SubPmts")]
    public List<SubPmt> SubPmts { get; set; } = new();

    [JsonPropertyName("AdditionalInfo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AdditionalInfo? AdditionalInfo { get; set; }

    [JsonPropertyName("BillCustomerCat")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BillCustomerCat { get; set; }
}
