using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DirectPayGateway.Core.DTOs.Ctm;
using DirectPayGateway.Core.DTOs.Ctm.Shared;
using DirectPayGateway.Core.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DirectPayGateway.API.Controllers;

/// <summary>
/// Development-only controller to simulate CTM calls for testing.
/// </summary>
[ApiController]
[Route("api/test")]
[Authorize(Roles = "Admin")]
public class CtmTestController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CtmTestController> _logger;

    public CtmTestController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<CtmTestController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Simulates a CTM Bill Pull request to our own endpoint.
    /// </summary>
    [HttpPost("simulate-bill-pull")]
    public async Task<IActionResult> SimulateBillPull([FromBody] SimulateBillPullRequest request)
    {
        if (!IsDevEnvironment())
        {
            return BadRequest(ApiResponse.Fail("NOT_ALLOWED", "Test endpoints only available in Development"));
        }

        var mfepRequest = new MfepBillPullRequest
        {
            MFEP = new BillPullMfepRequest
            {
                MsgHeader = new MsgHeader
                {
                    TmStp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GUID = Guid.NewGuid().ToString(),
                    TrsInf = new TrsInf
                    {
                        SdrCode = 1,
                        RcvCode = int.TryParse(_configuration["CtmIntegration:BillerCode"], out var bpCode) ? bpCode : 0,
                        ReqTyp = "BILPULRQ"
                    }
                },
                MsgBody = new BillPullRequestBody
                {
                    AcctInfo = new AcctInfo
                    {
                        BillingNo = request.CampaignCode,
                        BillNo = request.BillNo
                    },
                    ServiceType = "Donations",
                    PayerInfo = new PayerInfo
                    {
                        IdType = "NAT",
                        Id = request.PayerId ?? "9901234567",
                        Nation = "JO"
                    }
                }
            }
        };

        var result = await SendCtmRequest("/api/ctm/bill-pull", mfepRequest);

        return Ok(ApiResponse<object>.Ok(new
        {
            sentRequest = mfepRequest,
            response = result
        }, "Bill Pull simulation completed"));
    }

    /// <summary>
    /// Simulates a CTM Payment Notification to our own endpoint.
    /// </summary>
    [HttpPost("simulate-payment")]
    public async Task<IActionResult> SimulatePayment([FromBody] SimulatePaymentRequest request)
    {
        if (!IsDevEnvironment())
        {
            return BadRequest(ApiResponse.Fail("NOT_ALLOWED", "Test endpoints only available in Development"));
        }

        var mfepRequest = new MfepPaymentNotificationRequest
        {
            MFEP = new PaymentNotificationMfepRequest
            {
                MsgHeader = new MsgHeader
                {
                    TmStp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GUID = Guid.NewGuid().ToString(),
                    TrsInf = new TrsInf
                    {
                        SdrCode = 1,
                        RcvCode = int.TryParse(_configuration["CtmIntegration:BillerCode"], out var pnCode) ? pnCode : 0,
                        ReqTyp = "BLRPMTNTFRQ"
                    }
                },
                MsgBody = new PaymentNotificationRequestBody
                {
                    Transactions = new PaymentNotificationTransactions
                    {
                        TrxInf = new PaymentNotificationTrxInf
                        {
                            AcctInfo = new AcctInfo
                            {
                                BillingNo = request.CampaignCode,
                                BillNo = request.BillNo
                            },
                            JOEBPPSTrx = request.JOEBPPSTrx
                                ?? $"{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
                            BankTrxId = $"BNK{Random.Shared.Next(100000, 999999)}",
                            BankCode = int.TryParse(request.BankCode, out var bankCode) ? bankCode : 20,
                            PmtStatus = "Paid",
                            DueAmt = request.Amount.ToString("F3"),
                            PaidAmt = request.Amount.ToString("F3"),
                            FeesAmt = "0.000",
                            FeesOnBiller = false,
                            ProcessDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                            StmtDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                            Currency = "JOD",
                            AccessChannel = request.AccessChannel ?? "MobileBanking",
                            PaymentMethod = "CASH",
                            PaymentType = "PostPaid",
                            ServiceTypeDetails = new ServiceTypeDetails
                            {
                                ServiceType = "Donations"
                            },
                            PayerInfo = new PayerInfo
                            {
                                IdType = "NAT",
                                Id = request.PayerId ?? "9901234567",
                                Nation = "JO"
                            }
                        }
                    }
                }
            }
        };

        var result = await SendCtmRequest("/api/ctm/payment-notification", mfepRequest);

        return Ok(ApiResponse<object>.Ok(new
        {
            sentRequest = mfepRequest,
            response = result
        }, "Payment Notification simulation completed"));
    }

    /// <summary>
    /// Simulates a CTM Payment Acknowledgment to our own endpoint.
    /// </summary>
    [HttpPost("simulate-acknowledgment")]
    public async Task<IActionResult> SimulateAcknowledgment([FromBody] SimulateAcknowledgmentRequest request)
    {
        if (!IsDevEnvironment())
        {
            return BadRequest(ApiResponse.Fail("NOT_ALLOWED", "Test endpoints only available in Development"));
        }

        var mfepRequest = new MfepPaymentAcknowledgmentRequest
        {
            MFEP = new PaymentAcknowledgmentMfepRequest
            {
                MsgHeader = new MsgHeader
                {
                    TmStp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GUID = Guid.NewGuid().ToString(),
                    TrsInf = new TrsInf
                    {
                        SdrCode = 1,
                        RcvCode = int.TryParse(_configuration["CtmIntegration:BillerCode"], out var ackCode) ? ackCode : 0,
                        ResTyp = "PMTACKRQ"
                    }
                },
                MsgBody = new PaymentAcknowledgmentRequestBody
                {
                    BillingInfo = new BillingInfo
                    {
                        AcctInfo = new AcctInfo
                        {
                            BillingNo = request.CampaignCode,
                            BillerCode = int.TryParse(_configuration["CtmIntegration:BillerCode"], out var billerCode) ? billerCode : 0
                        },
                        JOEBPPSTrx = request.JOEBPPSTrx,
                        ParTrxId = $"PTX{Random.Shared.Next(100, 999)}",
                        PmtSrc = "Bank",
                        PmtStatus = "Paid",
                        DueAmt = request.Amount.ToString("F3"),
                        PaidAmt = request.Amount.ToString("F3"),
                        FeesAmt = "0.000",
                        FeesOnBiller = false,
                        ProcessDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                        StmtDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                        AccessChannel = "MobileBanking",
                        PaymentMethod = "CASH",
                        PaymentType = "PostPaid",
                        ServiceTypeDetails = new ServiceTypeDetails
                        {
                            ServiceType = "Donations"
                        }
                    }
                }
            }
        };

        var result = await SendCtmRequest("/api/ctm/payment-acknowledgment", mfepRequest);

        return Ok(ApiResponse<object>.Ok(new
        {
            sentRequest = mfepRequest,
            response = result
        }, "Payment Acknowledgment simulation completed"));
    }

    private async Task<JsonElement?> SendCtmRequest(string endpoint, object requestBody)
    {
        var client = _httpClientFactory.CreateClient();

        var username = _configuration["CtmIntegration:BasicAuth:Username"] ?? "";
        var password = _configuration["CtmIntegration:BasicAuth:Password"] ?? "";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

        var requestMessage = new HttpRequestMessage(HttpMethod.Post,
            $"{Request.Scheme}://{Request.Host}{endpoint}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        requestMessage.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        try
        {
            var response = await client.SendAsync(requestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("CTM Test simulation to {Endpoint}: Status={Status}",
                endpoint, response.StatusCode);

            return JsonSerializer.Deserialize<JsonElement>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CTM Test simulation failed for {Endpoint}", endpoint);
            return null;
        }
    }

    private bool IsDevEnvironment()
    {
        var env = _configuration["ASPNETCORE_ENVIRONMENT"]
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return env == "Development" || env == null; // Allow in dev or when env is not explicitly set
    }
}

public class SimulateBillPullRequest
{
    public string CampaignCode { get; set; } = "CAMP001";
    public string? BillNo { get; set; } = "BILL001";
    public string? PayerId { get; set; }
}

public class SimulatePaymentRequest
{
    public string CampaignCode { get; set; } = "CAMP001";
    public string? BillNo { get; set; } = "BILL001";
    public decimal Amount { get; set; } = 50.000m;
    public string? JOEBPPSTrx { get; set; }
    public string? PayerId { get; set; }
    public string? BankCode { get; set; }
    public string? AccessChannel { get; set; }
}

public class SimulateAcknowledgmentRequest
{
    public string CampaignCode { get; set; } = "CAMP001";
    public string JOEBPPSTrx { get; set; } = string.Empty;
    public decimal Amount { get; set; } = 50.000m;
}
