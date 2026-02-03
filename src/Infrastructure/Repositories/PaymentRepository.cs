using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using DirectPayGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DirectPayGateway.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction)
    {
        _context.PaymentTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<PaymentTransaction?> GetByIdAsync(int id)
    {
        return await _context.PaymentTransactions
            .Include(p => p.User)
            .Include(p => p.Logs)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PaymentTransaction?> GetByBillerTrxNoAsync(string billerTrxNo)
    {
        return await _context.PaymentTransactions
            .Include(p => p.User)
            .Include(p => p.Logs)
            .FirstOrDefaultAsync(p => p.BillerTrxNo == billerTrxNo);
    }

    public async Task<PaymentTransaction> UpdateAsync(PaymentTransaction transaction)
    {
        transaction.UpdatedAt = DateTime.UtcNow;
        _context.PaymentTransactions.Update(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<PagedResult<PaymentTransaction>> GetPagedAsync(TransactionFilterRequest filter)
    {
        var query = _context.PaymentTransactions
            .Include(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.UserId))
        {
            query = query.Where(p => p.UserId == filter.UserId);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(p => p.Status == filter.Status.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= filter.ToDate.Value);
        }

        if (!string.IsNullOrEmpty(filter.BillerTrxNo))
        {
            query = query.Where(p => p.BillerTrxNo.Contains(filter.BillerTrxNo));
        }

        if (!string.IsNullOrEmpty(filter.DirectPayTrxNo))
        {
            query = query.Where(p => p.DirectPayTrxNo != null && p.DirectPayTrxNo.Contains(filter.DirectPayTrxNo));
        }

        if (filter.MinAmount.HasValue)
        {
            query = query.Where(p => p.Amount >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            query = query.Where(p => p.Amount <= filter.MaxAmount.Value);
        }

        var totalCount = await query.CountAsync();

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "amount" => filter.SortDescending ? query.OrderByDescending(p => p.Amount) : query.OrderBy(p => p.Amount),
            "status" => filter.SortDescending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            "billertrxno" => filter.SortDescending ? query.OrderByDescending(p => p.BillerTrxNo) : query.OrderBy(p => p.BillerTrxNo),
            _ => filter.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<PaymentTransaction>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<List<PaymentTransaction>> GetByUserIdAsync(string userId, int? limit = null)
    {
        var query = _context.PaymentTransactions
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt);

        if (limit.HasValue)
        {
            return await query.Take(limit.Value).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task AddLogAsync(TransactionLog log)
    {
        _context.TransactionLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByBillerTrxNoAsync(string billerTrxNo)
    {
        return await _context.PaymentTransactions
            .AnyAsync(p => p.BillerTrxNo == billerTrxNo);
    }
}
