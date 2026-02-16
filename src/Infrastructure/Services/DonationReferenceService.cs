using DirectPayGateway.Core.DTOs;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using DirectPayGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectPayGateway.Infrastructure.Services;

public class DonationReferenceService : IDonationReferenceService
{
    private readonly ApplicationDbContext _context;
    private readonly IDonationReferenceRepository _donationReferenceRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ILogger<DonationReferenceService> _logger;

    public DonationReferenceService(
        ApplicationDbContext context,
        IDonationReferenceRepository donationReferenceRepository,
        ICampaignRepository campaignRepository,
        ILogger<DonationReferenceService> logger)
    {
        _context = context;
        _donationReferenceRepository = donationReferenceRepository;
        _campaignRepository = campaignRepository;
        _logger = logger;
    }

    public async Task<CreateDonationReferenceResponse?> CreateReferenceAsync(CreateDonationReferenceRequest request)
    {
        var campaign = await _campaignRepository.GetByIdAsync(request.CampaignId);
        if (campaign == null)
        {
            _logger.LogWarning("CreateReference: Campaign not found. CampaignId: {CampaignId}", request.CampaignId);
            return null;
        }

        if (campaign.Status != "Active")
        {
            _logger.LogWarning("CreateReference: Campaign not active. CampaignId: {CampaignId}, Status: {Status}",
                request.CampaignId, campaign.Status);
            return null;
        }

        var billNo = campaign.BillNo ?? "";
        var prefix = billNo.Length >= 2 ? billNo.Substring(0, 2) : "00";

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var maxRef = await _donationReferenceRepository.GetMaxReferenceNumberWithPrefixAsync(prefix);
            long nextSeq = 1;
            if (!string.IsNullOrEmpty(maxRef) && maxRef.Length >= 10)
            {
                var suffix = maxRef.Substring(2);
                if (long.TryParse(suffix, out var parsed))
                    nextSeq = parsed + 1;
            }

            if (nextSeq > 99_999_999)
            {
                _logger.LogError("CreateReference: Sequence overflow for prefix {Prefix}", prefix);
                await transaction.RollbackAsync();
                return null;
            }

            var referenceNumber = prefix + nextSeq.ToString("D8");
            var expiresAt = DateTime.UtcNow.AddHours(24);

            var entity = new DonationReference
            {
                ReferenceNumber = referenceNumber,
                CampaignId = campaign.Id,
                IntendedAmount = request.Amount > 0 ? request.Amount : null,
                PayerEmail = request.Email,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            };

            await _donationReferenceRepository.CreateAsync(entity);
            await transaction.CommitAsync();

            _logger.LogInformation("CreateReference: Created {ReferenceNumber} for Campaign {CampaignId}",
                referenceNumber, request.CampaignId);

            return new CreateDonationReferenceResponse
            {
                ReferenceNumber = referenceNumber,
                CampaignName = campaign.NameAr,
                AssociationName = campaign.CustName,
                Amount = request.Amount,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "CreateReference failed for CampaignId: {CampaignId}", request.CampaignId);
            throw;
        }
    }
}
