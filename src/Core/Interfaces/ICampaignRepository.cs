using DirectPayGateway.Core.DTOs.Ctm;
using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;

namespace DirectPayGateway.Core.Interfaces;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(int id);
    Task<Campaign?> GetByCampaignCodeAsync(string campaignCode);
    Task<Campaign> CreateAsync(Campaign campaign);
    Task<Campaign> UpdateAsync(Campaign campaign);
    Task<PagedResult<Campaign>> GetPagedAsync(CampaignFilterRequest filter);
    Task<List<Campaign>> GetActiveCampaignsAsync();
    Task<int> GetCountAsync();
    Task BulkInsertAsync(IEnumerable<Campaign> campaigns);
}

public interface IDonationRepository
{
    Task<Donation?> GetByIdAsync(int id);
    Task<Donation?> GetByJOEBPPSTrxAsync(string joebppsTrx);
    Task<Donation> CreateAsync(Donation donation);
    Task<Donation> UpdateAsync(Donation donation);
    Task<PagedResult<Donation>> GetPagedAsync(DonationFilterRequest filter);
    Task<bool> ExistsByJOEBPPSTrxAsync(string joebppsTrx);
}

public interface ICtmAuditLogRepository
{
    Task<CtmAuditLog> CreateAsync(CtmAuditLog log);
    Task<PagedResult<CtmAuditLog>> GetPagedAsync(CtmAuditLogFilterRequest filter);
}

public interface IDonationReferenceRepository
{
    Task<DonationReference?> GetByReferenceNumberAsync(string referenceNumber);
    Task<DonationReference> CreateAsync(DonationReference donationReference);
    Task<DonationReference> UpdateAsync(DonationReference donationReference);
    Task<DonationReference?> GetByReferenceNumberForBillPullAsync(string referenceNumber);
    Task<string?> GetMaxReferenceNumberWithPrefixAsync(string prefix);
}
