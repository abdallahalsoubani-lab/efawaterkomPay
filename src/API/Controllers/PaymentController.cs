using DirectPayGateway.API.Extensions;
using DirectPayGateway.Core.DTOs.Common;
using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DirectPayGateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;
    private readonly IConfiguration _configuration;

    public PaymentController(
        PaymentService paymentService,
        ILogger<PaymentController> logger,
        IConfiguration configuration)
    {
        _paymentService = paymentService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("initiate")]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitiateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiatePayment([FromBody] PaymentInitiateRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse.Fail("UNAUTHORIZED", "User not authenticated"));
        }

        var clientIp = HttpContext.GetClientIpAddress();
        var result = await _paymentService.InitiatePaymentAsync(request, userId, clientIp);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<PaymentInitiateResponse>.Fail(
                "PAYMENT_INITIATION_FAILED",
                result.Message ?? "Failed to initiate payment"));
        }

        return Ok(ApiResponse<PaymentInitiateResponse>.Ok(result, "Payment initiated successfully"));
    }

    [AllowAnonymous]
    [HttpGet("callback")]
    public async Task<IActionResult> ProcessCallback([FromQuery] string ResponseParams)
    {
        var clientIp = HttpContext.GetClientIpAddress();

        _logger.LogInformation("Payment callback received: {ResponseParams}", ResponseParams);

        try
        {
            var result = await _paymentService.ProcessCallbackAsync(ResponseParams, clientIp);

            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            var redirectUrl = $"{frontendUrl}/payment/result?txn={result.Id}&status={result.Status}";

            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment callback processing failed");
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            return Redirect($"{frontendUrl}/payment/result?error=processing_failed");
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransaction(int id)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var transaction = await _paymentService.GetTransactionAsync(id, isAdmin ? null : userId);

        if (transaction == null)
        {
            return NotFound(ApiResponse.Fail("NOT_FOUND", "Transaction not found"));
        }

        return Ok(ApiResponse<TransactionDto>.Ok(transaction));
    }

    [HttpGet("detail/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionDetail(int id)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var transaction = await _paymentService.GetTransactionDetailAsync(id, isAdmin ? null : userId);

        if (transaction == null)
        {
            return NotFound(ApiResponse.Fail("NOT_FOUND", "Transaction not found"));
        }

        return Ok(ApiResponse<TransactionDetailDto>.Ok(transaction));
    }

    [HttpGet("status/{billerTrxNo}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionByBillerTrxNo(string billerTrxNo)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var transaction = await _paymentService.GetTransactionByBillerTrxNoAsync(
            billerTrxNo, isAdmin ? null : userId);

        if (transaction == null)
        {
            return NotFound(ApiResponse.Fail("NOT_FOUND", "Transaction not found"));
        }

        return Ok(ApiResponse<TransactionDto>.Ok(transaction));
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory([FromQuery] TransactionFilterRequest filter)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse.Fail("UNAUTHORIZED", "User not authenticated"));
        }

        var result = await _paymentService.GetUserTransactionsAsync(userId, filter);
        return Ok(ApiResponse<PagedResult<TransactionDto>>.Ok(result));
    }
}
