using System.Text;
using System.Web;
using DirectPayGateway.Core.Constants;
using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Exceptions;
using DirectPayGateway.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BC = BCrypt.Net.BCrypt;

namespace DirectPayGateway.Infrastructure.ExternalServices;

public class DirectPayService : IDirectPayService
{
    private readonly DirectPaySettings _settings;
    private readonly ILogger<DirectPayService> _logger;

    public DirectPayService(
        IOptions<DirectPaySettings> settings,
        ILogger<DirectPayService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<PaymentInitiationResult> InitiatePaymentAsync(DirectPayRequest request)
    {
        try
        {
            var requestUrl = BuildRequestUrl(request);

            _logger.LogInformation(
                "DirectPay payment initiated: BillerTrxNo={BillerTrxNo}, Amount={Amount}",
                request.BillerTrxNo, request.Amount);

            return Task.FromResult(new PaymentInitiationResult
            {
                Success = true,
                RedirectUrl = requestUrl,
                RequestHash = ExtractHashFromUrl(requestUrl)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate DirectPay payment: {BillerTrxNo}", request.BillerTrxNo);
            return Task.FromResult(new PaymentInitiationResult
            {
                Success = false,
                ErrorMessage = "Failed to initiate payment. Please try again."
            });
        }
    }

    public Task<PaymentCallbackResult> ProcessCallbackAsync(string responseParams)
    {
        try
        {
            _logger.LogInformation("Processing DirectPay callback: {ResponseParams}", responseParams);

            var decodedParams = HttpUtility.UrlDecode(responseParams);
            var parts = decodedParams.Split('|');

            if (parts.Length < 7)
            {
                _logger.LogWarning("Invalid callback format - insufficient parts: {Count}", parts.Length);
                throw new PaymentException("Invalid callback format");
            }

            var result = new PaymentCallbackResult
            {
                BillerTrxNo = parts[0],
                TrxStatus = ParseInt(parts[1]),
                DirectPayTrxNo = parts[2],
                Amount = ParseDecimal(parts[3]),
                PaymentStatus = ParseInt(parts[4]),
                OtherDetails = parts[5],
                HashValid = true
            };

            var receivedHash = parts[6];
            var paramsWithoutHash = string.Join("|", parts.Take(6));

            if (!ValidateSecureHash(paramsWithoutHash, receivedHash))
            {
                _logger.LogWarning(
                    "Invalid secure hash for transaction: {BillerTrxNo}",
                    result.BillerTrxNo);
                result.HashValid = false;
            }

            result.Success = result.HashValid && result.TrxStatus == 1;

            if (result.TrxStatus.HasValue)
            {
                result.Message = DirectPayErrorCodes.GetMessage(result.TrxStatus.Value);
            }

            _logger.LogInformation(
                "DirectPay callback processed: BillerTrxNo={BillerTrxNo}, TrxStatus={TrxStatus}, PaymentStatus={PaymentStatus}",
                result.BillerTrxNo, result.TrxStatus, result.PaymentStatus);

            return Task.FromResult(result);
        }
        catch (PaymentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process DirectPay callback");
            throw new PaymentException("Failed to process payment callback", null);
        }
    }

    public bool ValidateSecureHash(string responseParams, string receivedHash)
    {
        try
        {
            var stringToVerify = responseParams + _settings.SecretToken;
            return BC.Verify(stringToVerify, receivedHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hash validation failed");
            return false;
        }
    }

    public string GenerateSecureHash(string requestParams)
    {
        var stringToHash = requestParams + _settings.SecretToken;
        return BC.HashPassword(stringToHash, BC.GenerateSalt(4));
    }

    public string BuildRequestUrl(DirectPayRequest request)
    {
        var requestParams = BuildRequestParams(request);
        var hash = GenerateSecureHash(requestParams);
        var fullParams = $"{requestParams}|{hash}";

        var baseUrl = _settings.GetActiveBaseUrl();
        var encodedParams = HttpUtility.UrlEncode(fullParams);

        return $"{baseUrl}?RequestParams={encodedParams}";
    }

    private string BuildRequestParams(DirectPayRequest request)
    {
        var sb = new StringBuilder();

        sb.Append(SanitizeInput(request.BillerTrxNo, 20));
        sb.Append('|');
        sb.Append(_settings.BillerCode);
        sb.Append('|');
        sb.Append(_settings.ServiceCode);
        sb.Append('|');
        sb.Append(request.PaymentType);
        sb.Append('|');
        sb.Append(_settings.Currency);
        sb.Append('|');
        sb.Append(SanitizeInput(request.BillingNo ?? string.Empty, 50));
        sb.Append('|');
        sb.Append(request.PrepaidCatCode?.ToString() ?? string.Empty);
        sb.Append('|');
        sb.Append(request.Amount.ToString("F3"));
        sb.Append('|');
        sb.Append(SanitizeInput(request.StatementNarrative ?? string.Empty, 100));
        sb.Append('|');
        sb.Append(SanitizeInput(request.CustomerEmail ?? string.Empty, 250));
        sb.Append('|');
        sb.Append(request.Language);
        sb.Append('|');
        sb.Append(SanitizeInput(request.OtherDetails ?? string.Empty, 250));

        return sb.ToString();
    }

    private static string SanitizeInput(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        char[] invalidChars = { '~', '"', '\'', '&', '#', '%', '|' };
        var sanitized = new string(input.Where(c => !invalidChars.Contains(c)).ToArray());

        if (sanitized.Length > maxLength)
            sanitized = sanitized[..maxLength];

        return sanitized;
    }

    private static string ExtractHashFromUrl(string url)
    {
        var paramsStart = url.IndexOf("RequestParams=", StringComparison.OrdinalIgnoreCase);
        if (paramsStart < 0) return string.Empty;

        var paramsValue = url[(paramsStart + 14)..];
        var decoded = HttpUtility.UrlDecode(paramsValue);
        var parts = decoded.Split('|');

        return parts.Length > 0 ? parts[^1] : string.Empty;
    }

    private static int? ParseInt(string value)
    {
        return int.TryParse(value, out var result) ? result : null;
    }

    private static decimal? ParseDecimal(string value)
    {
        return decimal.TryParse(value, out var result) ? result : null;
    }
}
