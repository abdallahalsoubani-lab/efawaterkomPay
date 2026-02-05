using DirectPayGateway.Core.DTOs.Ctm;
using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using DirectPayGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DirectPayGateway.Infrastructure.Repositories;

public class CampaignRepository : ICampaignRepository
{
    private readonly ApplicationDbContext _context;

    public CampaignRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Campaign?> GetByIdAsync(int id)
    {
        return await _context.Campaigns
            .Include(c => c.Donations)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Campaign?> GetByCampaignCodeAsync(string campaignCode)
    {
        return await _context.Campaigns
            .FirstOrDefaultAsync(c => c.CampaignCode == campaignCode);
    }

    public async Task<Campaign> CreateAsync(Campaign campaign)
    {
        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync();
        return campaign;
    }

    public async Task<Campaign> UpdateAsync(Campaign campaign)
    {
        _context.Campaigns.Update(campaign);
        await _context.SaveChangesAsync();
        return campaign;
    }

    public async Task<PagedResult<Campaign>> GetPagedAsync(CampaignFilterRequest filter)
    {
        var query = _context.Campaigns.AsQueryable();

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(c =>
                c.CampaignCode.ToLower().Contains(term) ||
                c.NameAr.Contains(term) ||
                c.NameEn.ToLower().Contains(term));
        }

        if (!string.IsNullOrEmpty(filter.Status))
        {
            query = query.Where(c => c.Status == filter.Status);
        }

        if (!string.IsNullOrEmpty(filter.Category))
        {
            query = query.Where(c => c.Category == filter.Category);
        }

        var totalCount = await query.CountAsync();

        query = filter.SortBy?.ToLower() switch
        {
            "name" => filter.SortDescending ? query.OrderByDescending(c => c.NameEn) : query.OrderBy(c => c.NameEn),
            "targetamount" => filter.SortDescending ? query.OrderByDescending(c => c.TargetAmount) : query.OrderBy(c => c.TargetAmount),
            "collectedamount" => filter.SortDescending ? query.OrderByDescending(c => c.CollectedAmount) : query.OrderBy(c => c.CollectedAmount),
            "donorscount" => filter.SortDescending ? query.OrderByDescending(c => c.DonorsCount) : query.OrderBy(c => c.DonorsCount),
            "startdate" => filter.SortDescending ? query.OrderByDescending(c => c.StartDate) : query.OrderBy(c => c.StartDate),
            "enddate" => filter.SortDescending ? query.OrderByDescending(c => c.EndDate) : query.OrderBy(c => c.EndDate),
            _ => filter.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Campaign>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<List<Campaign>> GetActiveCampaignsAsync()
    {
        return await _context.Campaigns
            .Where(c => c.Status == "Active" && c.EndDate > DateTime.UtcNow)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Campaigns.CountAsync();
    }

    public async Task BulkInsertAsync(IEnumerable<Campaign> campaigns)
    {
        await _context.Campaigns.AddRangeAsync(campaigns);
        await _context.SaveChangesAsync();
    }
}
