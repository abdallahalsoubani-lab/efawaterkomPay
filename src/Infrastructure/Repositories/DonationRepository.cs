using DirectPayGateway.Core.DTOs.Ctm;
using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using DirectPayGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DirectPayGateway.Infrastructure.Repositories;

public class DonationRepository : IDonationRepository
{
    private readonly ApplicationDbContext _context;

    public DonationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Donation?> GetByIdAsync(int id)
    {
        return await _context.Donations
            .Include(d => d.Campaign)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Donation?> GetByJOEBPPSTrxAsync(string joebppsTrx)
    {
        return await _context.Donations
            .Include(d => d.Campaign)
            .FirstOrDefaultAsync(d => d.JOEBPPSTrx == joebppsTrx);
    }

    public async Task<Donation> CreateAsync(Donation donation)
    {
        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();
        return donation;
    }

    public async Task<Donation> UpdateAsync(Donation donation)
    {
        _context.Donations.Update(donation);
        await _context.SaveChangesAsync();
        return donation;
    }

    public async Task<PagedResult<Donation>> GetPagedAsync(DonationFilterRequest filter)
    {
        var query = _context.Donations
            .Include(d => d.Campaign)
            .AsQueryable();

        if (filter.CampaignId.HasValue)
        {
            query = query.Where(d => d.CampaignId == filter.CampaignId.Value);
        }

        if (!string.IsNullOrEmpty(filter.JOEBPPSTrx))
        {
            query = query.Where(d => d.JOEBPPSTrx.Contains(filter.JOEBPPSTrx));
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(d => d.ProcessDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(d => d.ProcessDate <= filter.ToDate.Value);
        }

        if (filter.MinAmount.HasValue)
        {
            query = query.Where(d => d.PaidAmount >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            query = query.Where(d => d.PaidAmount <= filter.MaxAmount.Value);
        }

        if (!string.IsNullOrEmpty(filter.PmtStatus))
        {
            query = query.Where(d => d.PmtStatus == filter.PmtStatus);
        }

        var totalCount = await query.CountAsync();

        query = filter.SortBy?.ToLower() switch
        {
            "paidamount" => filter.SortDescending ? query.OrderByDescending(d => d.PaidAmount) : query.OrderBy(d => d.PaidAmount),
            "processdate" => filter.SortDescending ? query.OrderByDescending(d => d.ProcessDate) : query.OrderBy(d => d.ProcessDate),
            "campaign" => filter.SortDescending ? query.OrderByDescending(d => d.Campaign.NameEn) : query.OrderBy(d => d.Campaign.NameEn),
            _ => filter.SortDescending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt)
        };

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Donation>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<bool> ExistsByJOEBPPSTrxAsync(string joebppsTrx)
    {
        return await _context.Donations.AnyAsync(d => d.JOEBPPSTrx == joebppsTrx);
    }
}
