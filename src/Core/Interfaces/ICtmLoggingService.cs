using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;

namespace DirectPayGateway.Core.Interfaces;

public interface ICtmLoggingService
{
    Task<CtmApiLog> LogRequestAsync(string endpoint, string requestBody, string? clientIp, string? userAgent);
    Task UpdateLogWithResponseAsync(int logId, string responseBody, int statusCode, long responseTimeMs,
        string? billingNo = null, string? joebppsTrx = null, string? errorMessage = null);
    Task<PagedResult<CtmApiLog>> GetLogsPagedAsync(CtmApiLogQueryFilter filter);
}

public class CtmApiLogQueryFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Endpoint { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? BillingNo { get; set; }
    public string? JOEBPPSTrx { get; set; }
    public bool SortDescending { get; set; } = true;
}
