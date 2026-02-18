import api, { ApiResponse, PagedResult } from './api';

export interface Campaign {
  id: number;
  campaignCode: string;
  billNo: string;
  nameAr: string;
  nameEn: string;
  descriptionAr?: string;
  descriptionEn?: string;
  category: string;
  serviceType: string;
  status: string;
  billType: string;
  billCustomerCat: string;
  targetAmount: number;
  collectedAmount: number;
  donorsCount: number;
  startDate: string;
  endDate: string;
  allowPartialPayment: boolean;
  minAmount: number;
  maxAmount: number;
  iban: string;
  bankCode: string;
  custName?: string;
  freeText?: string;
  email?: string;
  phone?: string;
  createdAt: string;
  updatedAt?: string;
  progressPercentage: number;
}

export interface CampaignDetail extends Campaign {
  recentDonations: Donation[];
}

export interface Donation {
  id: number;
  campaignId: number;
  campaignName?: string;
  campaignCode?: string;
  joebppsTrx: string;
  bankTrxId?: string;
  bankCode?: string;
  billingNo: string;
  dueAmount: number;
  paidAmount: number;
  feesAmount: number;
  feesOnBiller: boolean;
  pmtStatus: string;
  currency: string;
  accessChannel?: string;
  paymentMethod?: string;
  paymentType?: string;
  processDate: string;
  stmtDate: string;
  payerIdType?: string;
  payerId?: string;
  payerNation?: string;
  payerName?: string;
  payerPhone?: string;
  payerEmail?: string;
  isAcknowledged: boolean;
  createdAt: string;
}

export interface CtmAuditLog {
  id: number;
  apiName: string;
  guid?: string;
  requestType?: string;
  requestBody?: string;
  responseBody?: string;
  responseCode: number;
  ipAddress?: string;
  timestamp: string;
  durationMs: number;
}

export interface CampaignFilter {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  status?: string;
  category?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface DonationFilter {
  page?: number;
  pageSize?: number;
  campaignId?: number;
  joebppsTrx?: string;
  fromDate?: string;
  toDate?: string;
  minAmount?: number;
  maxAmount?: number;
  pmtStatus?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface CtmLogFilter {
  page?: number;
  pageSize?: number;
  apiName?: string;
  fromDate?: string;
  toDate?: string;
  responseCode?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface CreateCampaignRequest {
  campaignCode: string;
  billNo: string;
  nameAr: string;
  nameEn: string;
  descriptionAr?: string;
  descriptionEn?: string;
  category: string;
  serviceType: string;
  status: string;
  billType: string;
  billCustomerCat: string;
  targetAmount: number;
  startDate: string;
  endDate: string;
  allowPartialPayment: boolean;
  minAmount: number;
  maxAmount: number;
  iban: string;
  bankCode: string;
  custName?: string;
  freeText?: string;
  email?: string;
  phone?: string;
}

export interface UpdateCampaignRequest {
  nameAr?: string;
  nameEn?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  category?: string;
  status?: string;
  targetAmount?: number;
  startDate?: string;
  endDate?: string;
  allowPartialPayment?: boolean;
  minAmount?: number;
  maxAmount?: number;
  iban?: string;
  bankCode?: string;
  custName?: string;
  freeText?: string;
  email?: string;
  phone?: string;
}

function buildParams(filter: Record<string, any>): string {
  const params = new URLSearchParams();
  Object.entries(filter).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      params.append(key, String(value));
    }
  });
  return params.toString();
}

export interface CtmApiLog {
  id: number;
  timestamp: string;
  endpoint: string;
  httpMethod: string;
  requestBody?: string;
  responseBody?: string;
  httpStatusCode: number;
  billingNo?: string;
  joebppsTrx?: string;
  errorMessage?: string;
  clientIp?: string;
  userAgent?: string;
  responseTimeMs: number;
}

export interface CtmApiLogFilter {
  page?: number;
  pageSize?: number;
  endpoint?: string;
  fromDate?: string;
  toDate?: string;
  httpStatusCode?: number;
  billingNo?: string;
  joebppsTrx?: string;
  sortDescending?: boolean;
}

export const campaignService = {
  async getCampaigns(filter: CampaignFilter = {}): Promise<PagedResult<Campaign>> {
    const response = await api.get<ApiResponse<PagedResult<Campaign>>>(
      `/admin/campaigns?${buildParams(filter)}`
    );
    return response.data.data!;
  },

  async getCampaign(id: number): Promise<CampaignDetail> {
    const response = await api.get<ApiResponse<CampaignDetail>>(`/admin/campaigns/${id}`);
    return response.data.data!;
  },

  async createCampaign(data: CreateCampaignRequest): Promise<Campaign> {
    const response = await api.post<ApiResponse<Campaign>>('/admin/campaigns', data);
    return response.data.data!;
  },

  async updateCampaign(id: number, data: UpdateCampaignRequest): Promise<Campaign> {
    const response = await api.put<ApiResponse<Campaign>>(`/admin/campaigns/${id}`, data);
    return response.data.data!;
  },

  async getDonations(filter: DonationFilter = {}): Promise<PagedResult<Donation>> {
    const response = await api.get<ApiResponse<PagedResult<Donation>>>(
      `/admin/donations?${buildParams(filter)}`
    );
    return response.data.data!;
  },

  async exportDonations(fromDate?: string, toDate?: string, campaignId?: number): Promise<Blob> {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    if (campaignId) params.append('campaignId', String(campaignId));
    const response = await api.get(`/admin/donations/export?${params}`, {
      responseType: 'blob',
    });
    return response.data;
  },

  async getCtmLogs(filter: CtmLogFilter = {}): Promise<PagedResult<CtmAuditLog>> {
    const response = await api.get<ApiResponse<PagedResult<CtmAuditLog>>>(
      `/admin/ctm-logs?${buildParams(filter)}`
    );
    return response.data.data!;
  },

  async getCtmApiLogs(filter: CtmApiLogFilter = {}): Promise<PagedResult<CtmApiLog>> {
    const response = await api.get<ApiResponse<PagedResult<CtmApiLog>>>(
      `/admin/ctm-api-logs?${buildParams(filter)}`
    );
    return response.data.data!;
  },
};

export default campaignService;
