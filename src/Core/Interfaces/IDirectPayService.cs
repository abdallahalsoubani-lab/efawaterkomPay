using DirectPayGateway.Core.DTOs.Payment;

namespace DirectPayGateway.Core.Interfaces;

public interface IDirectPayService
{
    Task<PaymentInitiationResult> InitiatePaymentAsync(DirectPayRequest request);
    Task<PaymentCallbackResult> ProcessCallbackAsync(string responseParams);
    bool ValidateSecureHash(string responseParams, string receivedHash);
    string GenerateSecureHash(string requestParams);
    string BuildRequestUrl(DirectPayRequest request);
}

public class DirectPayRequest
{
    public string BillerTrxNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PaymentType { get; set; }
    public string? BillingNo { get; set; }
    public int? PrepaidCatCode { get; set; }
    public string? CustomerEmail { get; set; }
    public string? StatementNarrative { get; set; }
    public string? OtherDetails { get; set; }
    public string Language { get; set; } = "AR";
}

public class PaymentInitiationResult
{
    public bool Success { get; set; }
    public string? RedirectUrl { get; set; }
    public string? RequestHash { get; set; }
    public string? ErrorMessage { get; set; }
}
