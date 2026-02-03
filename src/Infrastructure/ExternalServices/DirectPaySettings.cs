namespace DirectPayGateway.Infrastructure.ExternalServices;

public class DirectPaySettings
{
    public string BillerCode { get; set; } = string.Empty;
    public string ServiceCode { get; set; } = string.Empty;
    public string SecretToken { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public BaseUrlSettings BaseUrl { get; set; } = new();
    public string Environment { get; set; } = "Staging";
    public string Currency { get; set; } = "JOD";
    public string Language { get; set; } = "AR";

    public string GetActiveBaseUrl()
    {
        return Environment.Equals("Production", StringComparison.OrdinalIgnoreCase)
            ? BaseUrl.Production
            : BaseUrl.Staging;
    }
}

public class BaseUrlSettings
{
    public string Staging { get; set; } = "https://staging.efawateercom.jo/DirectPayService/DirectPay.aspx";
    public string Production { get; set; } = "https://www.efawateercom.jo/DirectPayService/DirectPay.aspx";
}
