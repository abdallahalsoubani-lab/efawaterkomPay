using System.Diagnostics;
using System.Text.Json;
using DirectPayGateway.API.Extensions;
using DirectPayGateway.Core.DTOs.Ctm;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DirectPayGateway.API.Controllers;

[ApiController]
[Route("api/ctm")]
[Authorize(AuthenticationSchemes = "CtmBasic")]
public class CtmBillerController : ControllerBase
{
    private readonly ICtmBillerService _ctmService;
    private readonly ICtmAuditLogRepository _auditLogRepository;
    private readonly ICtmLoggingService _ctmLoggingService;
    private readonly ILogger<CtmBillerController> _logger;

    public CtmBillerController(
        ICtmBillerService ctmService,
        ICtmAuditLogRepository auditLogRepository,
        ICtmLoggingService ctmLoggingService,
        ILogger<CtmBillerController> logger)
    {
        _ctmService = ctmService;
        _auditLogRepository = auditLogRepository;
        _ctmLoggingService = ctmLoggingService;
        _logger = logger;
    }

    [HttpPost("bill-pull")]
    [Produces("application/json")]
    public async Task<IActionResult> BillPull([FromBody] MfepBillPullRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestBody = JsonSerializer.Serialize(request);
        var guid = request.MFEP?.MsgHeader?.GUID ?? "unknown";
        var ipAddress = HttpContext.GetClientIpAddress();
        var userAgent = HttpContext.GetUserAgent();
        var billingNo = request.MFEP?.MsgBody?.AcctInfo?.BillingNo;

        _logger.LogInformation("CTM BillPull received. GUID: {GUID}, IP: {IP}", guid, ipAddress);

        var apiLog = await _ctmLoggingService.LogRequestAsync("bill-pull", requestBody, ipAddress, userAgent);

        MfepBillPullResponse response;
        int errorCode = 0;
        string? errorMessage = null;

        try
        {
            response = await _ctmService.HandleBillPullAsync(request);
            errorCode = response.MFEP.MsgHeader.Result?.ErrorCode ?? 0;
            if (errorCode != 0)
                errorMessage = response.MFEP.MsgHeader.Result?.ErrorDesc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CTM BillPull error. GUID: {GUID}", guid);
            errorCode = 5;
            errorMessage = ex.Message;
            response = BuildBillPullInternalErrorResponse(guid);
        }

        stopwatch.Stop();

        var responseBody = JsonSerializer.Serialize(response);

        await _auditLogRepository.CreateAsync(new CtmAuditLog
        {
            ApiName = "BillPull",
            GUID = guid,
            RequestType = "BILPULRQ",
            RequestBody = requestBody,
            ResponseBody = responseBody,
            ResponseCode = errorCode,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow,
            DurationMs = stopwatch.ElapsedMilliseconds
        });

        await _ctmLoggingService.UpdateLogWithResponseAsync(
            apiLog.Id, responseBody, errorCode == 0 ? 200 : 400, stopwatch.ElapsedMilliseconds,
            billingNo: billingNo, errorMessage: errorMessage);

        return Ok(response);
    }

    [HttpPost("payment-notification")]
    [Produces("application/json")]
    public async Task<IActionResult> PaymentNotification([FromBody] MfepPaymentNotificationRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestBody = JsonSerializer.Serialize(request);
        var guid = request.MFEP?.MsgHeader?.GUID ?? "unknown";
        var ipAddress = HttpContext.GetClientIpAddress();
        var userAgent = HttpContext.GetUserAgent();
        var billingNo = request.MFEP?.MsgBody?.Transactions?.TrxInf?.AcctInfo?.BillingNo;
        var joebppsTrx = request.MFEP?.MsgBody?.Transactions?.TrxInf?.JOEBPPSTrx;

        _logger.LogInformation("CTM PaymentNotification received. GUID: {GUID}, IP: {IP}", guid, ipAddress);

        var apiLog = await _ctmLoggingService.LogRequestAsync("payment-notification", requestBody, ipAddress, userAgent);

        MfepPaymentNotificationResponse response;
        int errorCode = 0;
        string? errorMessage = null;

        try
        {
            response = await _ctmService.HandlePaymentNotificationAsync(request);
            errorCode = response.MFEP.MsgHeader.Result?.ErrorCode ?? 0;
            if (errorCode != 0)
                errorMessage = response.MFEP.MsgHeader.Result?.ErrorDesc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CTM PaymentNotification error. GUID: {GUID}", guid);
            errorCode = 5;
            errorMessage = ex.Message;
            response = BuildPaymentNotificationInternalErrorResponse(guid, request);
        }

        stopwatch.Stop();

        var responseBody = JsonSerializer.Serialize(response);

        await _auditLogRepository.CreateAsync(new CtmAuditLog
        {
            ApiName = "PaymentNotification",
            GUID = guid,
            RequestType = "BLRPMTNTFRQ",
            RequestBody = requestBody,
            ResponseBody = responseBody,
            ResponseCode = errorCode,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow,
            DurationMs = stopwatch.ElapsedMilliseconds
        });

        await _ctmLoggingService.UpdateLogWithResponseAsync(
            apiLog.Id, responseBody, errorCode == 0 ? 200 : 400, stopwatch.ElapsedMilliseconds,
            billingNo: billingNo, joebppsTrx: joebppsTrx, errorMessage: errorMessage);

        return Ok(response);
    }

    [HttpPost("payment-acknowledgment")]
    [Produces("application/json")]
    public async Task<IActionResult> PaymentAcknowledgment([FromBody] MfepPaymentAcknowledgmentRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestBody = JsonSerializer.Serialize(request);
        var guid = request.MFEP?.MsgHeader?.GUID ?? "unknown";
        var ipAddress = HttpContext.GetClientIpAddress();
        var userAgent = HttpContext.GetUserAgent();
        var billingNo = request.MFEP?.MsgBody?.BillingInfo?.AcctInfo?.BillingNo;
        var joebppsTrx = request.MFEP?.MsgBody?.BillingInfo?.JOEBPPSTrx;

        _logger.LogInformation("CTM PaymentAcknowledgment received. GUID: {GUID}, IP: {IP}", guid, ipAddress);

        var apiLog = await _ctmLoggingService.LogRequestAsync("payment-acknowledgment", requestBody, ipAddress, userAgent);

        MfepPaymentAcknowledgmentResponse response;
        int errorCode = 0;
        string? errorMessage = null;

        try
        {
            response = await _ctmService.HandlePaymentAcknowledgmentAsync(request);
            errorCode = response.MFEP.MsgHeader.Result?.ErrorCode ?? 0;
            if (errorCode != 0)
                errorMessage = response.MFEP.MsgHeader.Result?.ErrorDesc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CTM PaymentAcknowledgment error. GUID: {GUID}", guid);
            errorCode = 5;
            errorMessage = ex.Message;
            response = BuildPaymentAcknowledgmentInternalErrorResponse(guid, request);
        }

        stopwatch.Stop();

        var responseBody = JsonSerializer.Serialize(response);

        await _auditLogRepository.CreateAsync(new CtmAuditLog
        {
            ApiName = "PaymentAcknowledgment",
            GUID = guid,
            RequestType = "PMTACKRQ",
            RequestBody = requestBody,
            ResponseBody = responseBody,
            ResponseCode = errorCode,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow,
            DurationMs = stopwatch.ElapsedMilliseconds
        });

        await _ctmLoggingService.UpdateLogWithResponseAsync(
            apiLog.Id, responseBody, errorCode == 0 ? 200 : 400, stopwatch.ElapsedMilliseconds,
            billingNo: billingNo, joebppsTrx: joebppsTrx, errorMessage: errorMessage);

        return Ok(response);
    }

    private static MfepBillPullResponse BuildBillPullInternalErrorResponse(string guid)
    {
        return new MfepBillPullResponse
        {
            MFEP = new BillPullMfepResponse
            {
                MsgHeader = new Core.DTOs.Ctm.Shared.MsgHeader
                {
                    TmStp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GUID = guid,
                    TrsInf = new Core.DTOs.Ctm.Shared.TrsInf { ResTyp = "BILPULRS" },
                    Result = new Core.DTOs.Ctm.Shared.MfepResult
                    {
                        ErrorCode = 5,
                        ErrorDesc = "Internal error",
                        Severity = "Error"
                    }
                },
                MsgBody = new BillPullResponseBody { RecCount = 0, BillRec = new() }
            }
        };
    }

    private static MfepPaymentNotificationResponse BuildPaymentNotificationInternalErrorResponse(
        string guid, MfepPaymentNotificationRequest request)
    {
        var trxInf = request.MFEP?.MsgBody?.Transactions?.TrxInf;
        return new MfepPaymentNotificationResponse
        {
            MFEP = new PaymentNotificationMfepResponse
            {
                MsgHeader = new Core.DTOs.Ctm.Shared.MsgHeader
                {
                    TmStp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GUID = guid,
                    TrsInf = new Core.DTOs.Ctm.Shared.TrsInf { ResTyp = "BLRPMTNTFRS" },
                    Result = new Core.DTOs.Ctm.Shared.MfepResult
                    {
                        ErrorCode = 5,
                        ErrorDesc = "Internal error",
                        Severity = "Error"
                    }
                },
                MsgBody = new PaymentNotificationResponseBody
                {
                    Transactions = new PaymentNotificationResponseTransactions
                    {
                        TrxInf = new PaymentNotificationResponseTrxInf
                        {
                            JOEBPPSTrx = trxInf?.JOEBPPSTrx ?? "",
                            ProcessDate = trxInf?.ProcessDate ?? "",
                            STMTDate = trxInf?.StmtDate ?? "",
                            Result = new Core.DTOs.Ctm.Shared.MfepResult
                            {
                                ErrorCode = 5,
                                ErrorDesc = "Internal error",
                                Severity = "Error"
                            }
                        }
                    }
                }
            }
        };
    }

    private static MfepPaymentAcknowledgmentResponse BuildPaymentAcknowledgmentInternalErrorResponse(
        string guid, MfepPaymentAcknowledgmentRequest request)
    {
        var billingInfo = request.MFEP?.MsgBody?.BillingInfo;
        return new MfepPaymentAcknowledgmentResponse
        {
            MFEP = new PaymentAcknowledgmentMfepResponse
            {
                MsgHeader = new Core.DTOs.Ctm.Shared.MsgHeader
                {
                    TmStp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GUID = guid,
                    TrsInf = new Core.DTOs.Ctm.Shared.TrsInf { ResTyp = "PMTACKRS" },
                    Result = new Core.DTOs.Ctm.Shared.MfepResult
                    {
                        ErrorCode = 5,
                        ErrorDesc = "Internal error",
                        Severity = "Error"
                    }
                },
                MsgBody = new PaymentAcknowledgmentResponseBody
                {
                    Transactions = new PaymentAcknowledgmentResponseTransactions
                    {
                        TrxInf = new PaymentAcknowledgmentResponseTrxInf
                        {
                            JOEBPPSTrx = billingInfo?.JOEBPPSTrx ?? "",
                            ProcessDate = billingInfo?.ProcessDate ?? "",
                            STMTDate = billingInfo?.StmtDate ?? "",
                            Result = new Core.DTOs.Ctm.Shared.MfepResult
                            {
                                ErrorCode = 5,
                                ErrorDesc = "Internal error",
                                Severity = "Error"
                            }
                        }
                    }
                }
            }
        };
    }
}
