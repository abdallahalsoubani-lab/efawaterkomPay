using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using DirectPayGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DirectPayGateway.Infrastructure.Services;

public class CtmLoggingService : ICtmLoggingService
{
    private readonly ApplicationDbContext _context;

    public CtmLoggingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CtmApiLog> LogRequestAsync(string endpoint, string requestBody, string? clientIp, string? userAgent)
    {
        var log = new CtmApiLog
        {
            Timestamp = DateTime.UtcNow,
            Endpoint = endpoint,
            HttpMethod = "POST",
            RequestBody = requestBody,
            ClientIp = clientIp,
            UserAgent = userAgent
        };

        _context.CtmApiLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task UpdateLogWithResponseAsync(int logId, string responseBody, int statusCode, long responseTimeMs,
        string? billingNo = null, string? joebppsTrx = null, string? errorMessage = null)
    {
        var log = await _context.CtmApiLogs.FindAsync(logId);
        if (log == null) return;

        log.ResponseBody = responseBody;
        log.HttpStatusCode = statusCode;
        log.ResponseTimeMs = responseTimeMs;
        log.BillingNo = billingNo;
        log.JOEBPPSTrx = joebppsTrx;
        log.ErrorMessage = errorMessage;
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<CtmApiLog>> GetLogsPagedAsync(CtmApiLogQueryFilter filter)
    {
        var query = _context.CtmApiLogs.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Endpoint))
            query = query.Where(l => l.Endpoint == filter.Endpoint);

        if (filter.FromDate.HasValue)
            query = query.Where(l => l.Timestamp >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(l => l.Timestamp <= filter.ToDate.Value);

        if (filter.HttpStatusCode.HasValue)
            query = query.Where(l => l.HttpStatusCode == filter.HttpStatusCode.Value);

        if (!string.IsNullOrEmpty(filter.BillingNo))
            query = query.Where(l => l.BillingNo != null && l.BillingNo.Contains(filter.BillingNo));

        if (!string.IsNullOrEmpty(filter.JOEBPPSTrx))
            query = query.Where(l => l.JOEBPPSTrx != null && l.JOEBPPSTrx.Contains(filter.JOEBPPSTrx));

        var totalCount = await query.CountAsync();

        query = filter.SortDescending
            ? query.OrderByDescending(l => l.Timestamp)
            : query.OrderBy(l => l.Timestamp);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<CtmApiLog>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }
}
