using DirectPayGateway.Core.Entities;
using DirectPayGateway.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DirectPayGateway.API.Controllers;

[ApiController]
[Route("api/donation-references")]
[AllowAnonymous]
public class DonationReferencesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DonationReferencesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Generate a unique reference number for Bank App payment. Valid for 24 hours.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDonationReferenceRequest request)
    {
        if (request == null || request.CampaignId <= 0)
            return BadRequest("CampaignId is required");

        // 1. Find campaign
        var campaign = await _context.Campaigns.FindAsync(request.CampaignId);
        if (campaign == null)
            return NotFound("Campaign not found");

        if (campaign.Status != "Active")
            return NotFound("Campaign not active");

        // 2. Generate reference number (10 digits: first 2 from BillNo + 8 digit sequence)
        var prefix = campaign.BillNo?.Length >= 2 ? campaign.BillNo.Substring(0, 2) : "00";
        var lastRef = await _context.DonationReferences
            .Where(r => r.ReferenceNumber.StartsWith(prefix))
            .OrderByDescending(r => r.ReferenceNumber)
            .FirstOrDefaultAsync();

        long nextSeq = 1;
        if (lastRef != null && lastRef.ReferenceNumber.Length >= 10)
        {
            if (long.TryParse(lastRef.ReferenceNumber.Substring(2), out var lastSeq))
                nextSeq = lastSeq + 1;
        }

        var referenceNumber = prefix + nextSeq.ToString("D8"); // e.g. "4200000001"

        // 3. Create reference record
        var reference = new DonationReference
        {
            ReferenceNumber = referenceNumber,
            CampaignId = request.CampaignId,
            IntendedAmount = request.Amount > 0 ? request.Amount : null,
            PayerEmail = request.Email,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
        };

        _context.DonationReferences.Add(reference);
        await _context.SaveChangesAsync();

        // 4. Return response
        return Ok(new
        {
            referenceNumber = reference.ReferenceNumber,
            campaignName = campaign.NameAr,
            associationName = campaign.CustName,
            amount = reference.IntendedAmount ?? request.Amount,
            expiresAt = reference.ExpiresAt,
        });
    }
}

public class CreateDonationReferenceRequest
{
    public int CampaignId { get; set; }
    public decimal Amount { get; set; }
    public string? Email { get; set; }
}
