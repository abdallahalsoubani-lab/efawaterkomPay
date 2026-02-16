using DirectPayGateway.Core.DTOs;

namespace DirectPayGateway.Core.Interfaces;

public interface IDonationReferenceService
{
    Task<CreateDonationReferenceResponse?> CreateReferenceAsync(CreateDonationReferenceRequest request);
}
