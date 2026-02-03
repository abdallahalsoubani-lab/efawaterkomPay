using DirectPayGateway.Core.Constants;
using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Exceptions;
using DirectPayGateway.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DirectPayGateway.Core.Services;

public class PaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDirectPayService _directPayService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IDirectPayService directPayService,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _directPayService = directPayService;
        _logger = logger;
    }

    public async Task<PaymentInitiateResponse> InitiatePaymentAsync(
        PaymentInitiateRequest request,
        string userId,
        string? clientIp = null)
    {
        ValidatePaymentRequest(request);

        var billerTrxNo = GenerateBillerTrxNo();

        var transaction = new PaymentTransaction
        {
            BillerTrxNo = billerTrxNo,
            UserId = userId,
            Amount = request.Amount,
            Currency = "JOD",
            BillingNo = request.BillingNo,
            PaymentType = request.PaymentType,
            PrepaidCatCode = request.PrepaidCatCode,
            CustomerEmail = request.CustomerEmail,
            StatementNarrative = request.StatementNarrative,
            OtherDetails = request.OtherDetails,
            Status = PaymentStatus.Pending,
            ClientIpAddress = clientIp
        };

        await _paymentRepository.CreateAsync(transaction);

        await AddLogAsync(transaction.Id, "INITIATED", "Payment initiated", clientIp);

        var directPayRequest = new DirectPayRequest
        {
            BillerTrxNo = billerTrxNo,
            Amount = request.Amount,
            PaymentType = request.PaymentType,
            BillingNo = request.BillingNo,
            PrepaidCatCode = request.PrepaidCatCode,
            CustomerEmail = request.CustomerEmail,
            StatementNarrative = request.StatementNarrative,
            OtherDetails = request.OtherDetails,
            Language = request.Language
        };

        var result = await _directPayService.InitiatePaymentAsync(directPayRequest);

        transaction.RequestUrl = result.RedirectUrl;
        transaction.RequestHash = result.RequestHash;
        transaction.Status = PaymentStatus.Processing;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(transaction);

        _logger.LogInformation("Payment initiated: {BillerTrxNo} for user {UserId}, Amount: {Amount}",
            billerTrxNo, userId, request.Amount);

        return new PaymentInitiateResponse
        {
            Success = true,
            TransactionId = transaction.Id.ToString(),
            BillerTrxNo = billerTrxNo,
            RedirectUrl = result.RedirectUrl,
            Message = "Payment initiated successfully"
        };
    }

    public async Task<TransactionDto> ProcessCallbackAsync(string responseParams, string? clientIp = null)
    {
        var callbackResult = await _directPayService.ProcessCallbackAsync(responseParams);

        if (string.IsNullOrEmpty(callbackResult.BillerTrxNo))
        {
            throw new PaymentException("Invalid callback: Missing transaction reference");
        }

        var transaction = await _paymentRepository.GetByBillerTrxNoAsync(callbackResult.BillerTrxNo);

        if (transaction == null)
        {
            throw new NotFoundException("PaymentTransaction", callbackResult.BillerTrxNo);
        }

        transaction.ResponseRaw = responseParams;
        transaction.TrxStatusCode = callbackResult.TrxStatus;
        transaction.DirectPayTrxNo = callbackResult.DirectPayTrxNo;
        transaction.DirectPayPaymentStatus = callbackResult.PaymentStatus;
        transaction.TrxStatusMessage = callbackResult.TrxStatus.HasValue
            ? DirectPayErrorCodes.GetMessage(callbackResult.TrxStatus.Value)
            : null;
        transaction.UpdatedAt = DateTime.UtcNow;

        if (callbackResult.PaymentStatus == 1 && callbackResult.TrxStatus == 1)
        {
            transaction.Status = PaymentStatus.Success;
            transaction.CompletedAt = DateTime.UtcNow;
            await AddLogAsync(transaction.Id, "COMPLETED", "Payment completed successfully", clientIp);
        }
        else if (callbackResult.PaymentStatus == 2)
        {
            transaction.Status = PaymentStatus.Processing;
            await AddLogAsync(transaction.Id, "PROCESSING", "Payment under processing", clientIp);
        }
        else if (callbackResult.PaymentStatus == 3 || callbackResult.TrxStatus == 12)
        {
            transaction.Status = callbackResult.TrxStatus == 12
                ? PaymentStatus.Cancelled
                : PaymentStatus.Failed;
            transaction.CompletedAt = DateTime.UtcNow;
            await AddLogAsync(transaction.Id, "FAILED",
                $"Payment failed: {transaction.TrxStatusMessage}", clientIp);
        }
        else
        {
            transaction.Status = PaymentStatus.Failed;
            transaction.CompletedAt = DateTime.UtcNow;
            await AddLogAsync(transaction.Id, "FAILED",
                $"Unknown status: TrxStatus={callbackResult.TrxStatus}, PaymentStatus={callbackResult.PaymentStatus}",
                clientIp);
        }

        await _paymentRepository.UpdateAsync(transaction);

        _logger.LogInformation(
            "Payment callback processed: {BillerTrxNo}, Status: {Status}, TrxStatus: {TrxStatus}",
            transaction.BillerTrxNo, transaction.Status, callbackResult.TrxStatus);

        return MapToDto(transaction);
    }

    public async Task<TransactionDto?> GetTransactionAsync(int id, string? userId = null)
    {
        var transaction = await _paymentRepository.GetByIdAsync(id);

        if (transaction == null)
            return null;

        if (userId != null && transaction.UserId != userId)
            throw new ForbiddenException("You don't have access to this transaction");

        return MapToDto(transaction);
    }

    public async Task<TransactionDetailDto?> GetTransactionDetailAsync(int id, string? userId = null)
    {
        var transaction = await _paymentRepository.GetByIdAsync(id);

        if (transaction == null)
            return null;

        if (userId != null && transaction.UserId != userId)
            throw new ForbiddenException("You don't have access to this transaction");

        return MapToDetailDto(transaction);
    }

    public async Task<TransactionDto?> GetTransactionByBillerTrxNoAsync(string billerTrxNo, string? userId = null)
    {
        var transaction = await _paymentRepository.GetByBillerTrxNoAsync(billerTrxNo);

        if (transaction == null)
            return null;

        if (userId != null && transaction.UserId != userId)
            throw new ForbiddenException("You don't have access to this transaction");

        return MapToDto(transaction);
    }

    public async Task<PagedResult<TransactionDto>> GetUserTransactionsAsync(
        string userId,
        TransactionFilterRequest filter)
    {
        filter.UserId = userId;
        var result = await _paymentRepository.GetPagedAsync(filter);

        return new PagedResult<TransactionDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<PagedResult<TransactionDto>> GetAllTransactionsAsync(TransactionFilterRequest filter)
    {
        var result = await _paymentRepository.GetPagedAsync(filter);

        return new PagedResult<TransactionDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    private void ValidatePaymentRequest(PaymentInitiateRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Amount <= 0)
        {
            errors["Amount"] = new[] { "Amount must be greater than zero" };
        }

        if (request.PaymentType == 1 && string.IsNullOrEmpty(request.BillingNo))
        {
            errors["BillingNo"] = new[] { "BillingNo is required for postpaid payments" };
        }

        if (request.PaymentType == 2 && !request.PrepaidCatCode.HasValue)
        {
            errors["PrepaidCatCode"] = new[] { "PrepaidCatCode is required for prepaid payments" };
        }

        if (ContainsInvalidCharacters(request.StatementNarrative) ||
            ContainsInvalidCharacters(request.OtherDetails) ||
            ContainsInvalidCharacters(request.BillingNo))
        {
            errors["Input"] = new[] { "Input contains invalid characters: ~ \" ' & # %" };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static bool ContainsInvalidCharacters(string? input)
    {
        if (string.IsNullOrEmpty(input)) return false;
        char[] invalidChars = { '~', '"', '\'', '&', '#', '%' };
        return input.IndexOfAny(invalidChars) >= 0;
    }

    private static string GenerateBillerTrxNo()
    {
        var timestamp = DateTime.UtcNow.ToString("yyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"{timestamp}{random}";
    }

    private async Task AddLogAsync(int transactionId, string action, string? details, string? clientIp)
    {
        var log = new TransactionLog
        {
            PaymentTransactionId = transactionId,
            Action = action,
            Details = details,
            IpAddress = clientIp,
            Timestamp = DateTime.UtcNow
        };
        await _paymentRepository.AddLogAsync(log);
    }

    private static TransactionDto MapToDto(PaymentTransaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            BillerTrxNo = transaction.BillerTrxNo,
            DirectPayTrxNo = transaction.DirectPayTrxNo,
            UserId = transaction.UserId,
            UserEmail = transaction.User?.Email,
            UserFullName = transaction.User?.FullName,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            BillingNo = transaction.BillingNo,
            PaymentType = transaction.PaymentType,
            PrepaidCatCode = transaction.PrepaidCatCode,
            CustomerEmail = transaction.CustomerEmail,
            StatementNarrative = transaction.StatementNarrative,
            Status = transaction.Status,
            TrxStatusCode = transaction.TrxStatusCode,
            TrxStatusMessage = transaction.TrxStatusMessage,
            CreatedAt = transaction.CreatedAt,
            CompletedAt = transaction.CompletedAt,
            UpdatedAt = transaction.UpdatedAt
        };
    }

    private static TransactionDetailDto MapToDetailDto(PaymentTransaction transaction)
    {
        return new TransactionDetailDto
        {
            Id = transaction.Id,
            BillerTrxNo = transaction.BillerTrxNo,
            DirectPayTrxNo = transaction.DirectPayTrxNo,
            UserId = transaction.UserId,
            UserEmail = transaction.User?.Email,
            UserFullName = transaction.User?.FullName,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            BillingNo = transaction.BillingNo,
            PaymentType = transaction.PaymentType,
            PrepaidCatCode = transaction.PrepaidCatCode,
            CustomerEmail = transaction.CustomerEmail,
            StatementNarrative = transaction.StatementNarrative,
            Status = transaction.Status,
            TrxStatusCode = transaction.TrxStatusCode,
            TrxStatusMessage = transaction.TrxStatusMessage,
            CreatedAt = transaction.CreatedAt,
            CompletedAt = transaction.CompletedAt,
            UpdatedAt = transaction.UpdatedAt,
            OtherDetails = transaction.OtherDetails,
            ClientIpAddress = transaction.ClientIpAddress,
            RequestUrl = transaction.RequestUrl,
            ResponseRaw = transaction.ResponseRaw,
            Logs = transaction.Logs.Select(l => new TransactionLogDto
            {
                Id = l.Id,
                Action = l.Action,
                Details = l.Details,
                Timestamp = l.Timestamp,
                IpAddress = l.IpAddress
            }).OrderByDescending(l => l.Timestamp).ToList()
        };
    }
}
