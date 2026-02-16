using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using DirectPayGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DirectPayGateway.Infrastructure.Repositories;

public class DonationReferenceRepository : IDonationReferenceRepository
{
    private readonly ApplicationDbContext _context;

    public DonationReferenceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DonationReference?> GetByReferenceNumberAsync(string referenceNumber)
    {
        return await _context.DonationReferences
            .Include(dr => dr.Campaign)
            .FirstOrDefaultAsync(dr => dr.ReferenceNumber == referenceNumber);
    }

    /// <summary>
    /// For Bill Pull: get reference with campaign, only if Pending and not expired.
    /// </summary>
    public async Task<DonationReference?> GetByReferenceNumberForBillPullAsync(string referenceNumber)
    {
        var now = DateTime.UtcNow;
        return await _context.DonationReferences
            .Include(dr => dr.Campaign)
            .FirstOrDefaultAsync(dr =>
                dr.ReferenceNumber == referenceNumber
                && dr.Status == "Pending"
                && dr.ExpiresAt > now);
    }

    public async Task<DonationReference> CreateAsync(DonationReference donationReference)
    {
        _context.DonationReferences.Add(donationReference);
        await _context.SaveChangesAsync();
        return donationReference;
    }

    public async Task<DonationReference> UpdateAsync(DonationReference donationReference)
    {
        _context.DonationReferences.Update(donationReference);
        await _context.SaveChangesAsync();
        return donationReference;
    }

    /// <summary>
    /// Returns the maximum reference number that starts with the given prefix (e.g. "42"), or null if none.
    /// Used with a transaction for thread-safe sequence generation.
    /// </summary>
    public async Task<string?> GetMaxReferenceNumberWithPrefixAsync(string prefix)
    {
        return await _context.DonationReferences
            .Where(dr => dr.ReferenceNumber.StartsWith(prefix))
            .MaxAsync(dr => (string?)dr.ReferenceNumber);
    }
}
