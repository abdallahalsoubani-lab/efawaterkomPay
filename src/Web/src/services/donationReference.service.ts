import api from './api';

export interface CreateDonationReferenceRequest {
  campaignId: number;
  amount: number;
  email?: string;
}

export interface CreateDonationReferenceResponse {
  referenceNumber: string;
  campaignName: string;
  associationName?: string;
  amount: number;
  expiresAt: string;
}

const donationReferenceService = {
  async create(
    request: CreateDonationReferenceRequest
  ): Promise<CreateDonationReferenceResponse> {
    const { data } = await api.post<CreateDonationReferenceResponse>(
      '/donation-references',
      request
    );
    return data;
  },
};

export { donationReferenceService };
export default donationReferenceService;
