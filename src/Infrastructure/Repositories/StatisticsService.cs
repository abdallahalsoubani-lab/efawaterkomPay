using DirectPayGateway.Core.DTOs.Admin;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using DirectPayGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DirectPayGateway.Infrastructure.Repositories;

public class StatisticsService : IStatisticsService
{
    private readonly ApplicationDbContext _context;

    public StatisticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatistics> GetDashboardStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var transactions = await _context.PaymentTransactions
            .Where(t => t.CreatedAt >= from && t.CreatedAt <= to)
            .ToListAsync();

        var totalTransactions = transactions.Count;
        var successfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Success);
        var failedTransactions = transactions.Count(t => t.Status == PaymentStatus.Failed);
        var pendingTransactions = transactions.Count(t => t.Status == PaymentStatus.Pending || t.Status == PaymentStatus.Processing);

        var totalAmount = transactions.Sum(t => t.Amount);
        var successfulAmount = transactions
            .Where(t => t.Status == PaymentStatus.Success)
            .Sum(t => t.Amount);

        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);

        var dailyStats = transactions
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new DailyStatistic
            {
                Date = g.Key,
                TransactionCount = g.Count(),
                TotalAmount = g.Sum(t => t.Amount),
                SuccessCount = g.Count(t => t.Status == PaymentStatus.Success),
                FailedCount = g.Count(t => t.Status == PaymentStatus.Failed)
            })
            .OrderBy(d => d.Date)
            .ToList();

        var statusBreakdown = new List<StatusBreakdown>();
        if (totalTransactions > 0)
        {
            foreach (PaymentStatus status in Enum.GetValues<PaymentStatus>())
            {
                var count = transactions.Count(t => t.Status == status);
                if (count > 0)
                {
                    statusBreakdown.Add(new StatusBreakdown
                    {
                        Status = status.ToString(),
                        Count = count,
                        Percentage = Math.Round((decimal)count / totalTransactions * 100, 2)
                    });
                }
            }
        }

        return new DashboardStatistics
        {
            TotalTransactions = totalTransactions,
            SuccessfulTransactions = successfulTransactions,
            FailedTransactions = failedTransactions,
            PendingTransactions = pendingTransactions,
            TotalAmount = totalAmount,
            SuccessfulAmount = successfulAmount,
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            DailyStatistics = dailyStats,
            StatusBreakdown = statusBreakdown
        };
    }
}
