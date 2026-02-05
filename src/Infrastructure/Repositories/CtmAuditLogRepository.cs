using DirectPayGateway.Core.DTOs.Ctm;
using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using DirectPayGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DirectPayGateway.Infrastructure.Repositories;

public class CtmAuditLogRepository : ICtmAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public CtmAuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CtmAuditLog> CreateAsync(CtmAuditLog log)
    {
        _context.CtmAuditLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<PagedResult<CtmAuditLog>> GetPagedAsync(CtmAuditLogFilterRequest filter)
    {
        var query = _context.CtmAuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(filter.ApiName))
        {
            query = query.Where(l => l.ApiName == filter.ApiName);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(l => l.Timestamp >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(l => l.Timestamp <= filter.ToDate.Value);
        }

        if (filter.ResponseCode.HasValue)
        {
            query = query.Where(l => l.ResponseCode == filter.ResponseCode.Value);
        }

        var totalCount = await query.CountAsync();

        query = filter.SortDescending
            ? query.OrderByDescending(l => l.Timestamp)
            : query.OrderBy(l => l.Timestamp);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<CtmAuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }
}
