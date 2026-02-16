namespace DirectPayGateway.Core.DTOs;

public class CreateDonationReferenceRequest
{
    public int CampaignId { get; set; }
    public decimal Amount { get; set; }
    public string? Email { get; set; }
}

public class CreateDonationReferenceResponse
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CampaignName { get; set; } = string.Empty;
    public string? AssociationName { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpiresAt { get; set; }
}
