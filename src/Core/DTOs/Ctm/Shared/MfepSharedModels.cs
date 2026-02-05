using System.Text.Json.Serialization;

namespace DirectPayGateway.Core.DTOs.Ctm.Shared;

public class MsgHeader
{
    [JsonPropertyName("TmStp")]
    public string TmStp { get; set; } = string.Empty;

    [JsonPropertyName("GUID")]
    public string GUID { get; set; } = string.Empty;

    [JsonPropertyName("TrsInf")]
    public TrsInf TrsInf { get; set; } = new();

    [JsonPropertyName("Result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MfepResult? Result { get; set; }
}

public class TrsInf
{
    [JsonPropertyName("SdrCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SdrCode { get; set; }

    [JsonPropertyName("RcvCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RcvCode { get; set; }

    [JsonPropertyName("ReqTyp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReqTyp { get; set; }

    [JsonPropertyName("ResTyp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ResTyp { get; set; }
}

public class MfepResult
{
    [JsonPropertyName("ErrorCode")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("ErrorDesc")]
    public string ErrorDesc { get; set; } = string.Empty;

    [JsonPropertyName("Severity")]
    public string Severity { get; set; } = "Info";
}

public class AcctInfo
{
    [JsonPropertyName("BillingNo")]
    public string BillingNo { get; set; } = string.Empty;

    [JsonPropertyName("BillNo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BillNo { get; set; }

    [JsonPropertyName("BillerCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BillerCode { get; set; }
}

public class PmtConst
{
    [JsonPropertyName("AllowPart")]
    public bool AllowPart { get; set; }

    [JsonPropertyName("Lower")]
    public string Lower { get; set; } = "0.000";

    [JsonPropertyName("Upper")]
    public string Upper { get; set; } = "0.000";
}

public class SubPmt
{
    [JsonPropertyName("Amount")]
    public string Amount { get; set; } = "0.000";

    [JsonPropertyName("SetBnkCode")]
    public string SetBnkCode { get; set; } = string.Empty;

    [JsonPropertyName("AcctNo")]
    public string AcctNo { get; set; } = string.Empty;
}

public class AdditionalInfo
{
    [JsonPropertyName("CustName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CustName { get; set; }

    [JsonPropertyName("FreeText")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FreeText { get; set; }

    [JsonPropertyName("Email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    [JsonPropertyName("Phone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Phone { get; set; }
}

public class PayerInfo
{
    [JsonPropertyName("IdType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IdType { get; set; }

    [JsonPropertyName("Id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("Nation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Nation { get; set; }
}

public class ServiceTypeDetails
{
    [JsonPropertyName("ServiceType")]
    public string ServiceType { get; set; } = string.Empty;
}
