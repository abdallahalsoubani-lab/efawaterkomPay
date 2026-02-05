using DirectPayGateway.Core.DTOs.Ctm;

namespace DirectPayGateway.Core.Interfaces;

public interface ICtmBillerService
{
    Task<MfepBillPullResponse> HandleBillPullAsync(MfepBillPullRequest request);
    Task<MfepPaymentNotificationResponse> HandlePaymentNotificationAsync(MfepPaymentNotificationRequest request);
    Task<MfepPaymentAcknowledgmentResponse> HandlePaymentAcknowledgmentAsync(MfepPaymentAcknowledgmentRequest request);
}
