import api, { ApiResponse, PagedResult } from './api';

export interface PaymentInitiateRequest {
  amount: number;
  paymentType: number;
  billingNo?: string;
  prepaidCatCode?: number;
  customerEmail?: string;
  statementNarrative?: string;
  otherDetails?: string;
  language: string;
}

export interface PaymentInitiateResponse {
  success: boolean;
  message?: string;
  transactionId?: string;
  billerTrxNo?: string;
  redirectUrl?: string;
}

export interface Transaction {
  id: number;
  billerTrxNo: string;
  directPayTrxNo?: string;
  userId: string;
  userEmail?: string;
  userFullName?: string;
  amount: number;
  currency: string;
  billingNo?: string;
  paymentType: number;
  paymentTypeName: string;
  prepaidCatCode?: number;
  customerEmail?: string;
  statementNarrative?: string;
  status: number;
  statusName: string;
  trxStatusCode?: number;
  trxStatusMessage?: string;
  createdAt: string;
  completedAt?: string;
  updatedAt?: string;
}

export interface TransactionDetail extends Transaction {
  otherDetails?: string;
  clientIpAddress?: string;
  requestUrl?: string;
  responseRaw?: string;
  logs: TransactionLog[];
}

export interface TransactionLog {
  id: number;
  action: string;
  details?: string;
  timestamp: string;
  ipAddress?: string;
}

export interface TransactionFilter {
  page?: number;
  pageSize?: number;
  status?: number;
  fromDate?: string;
  toDate?: string;
  billerTrxNo?: string;
  directPayTrxNo?: string;
  minAmount?: number;
  maxAmount?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export const paymentService = {
  async initiatePayment(data: PaymentInitiateRequest): Promise<PaymentInitiateResponse> {
    const response = await api.post<ApiResponse<PaymentInitiateResponse>>('/payment/initiate', data);
    return response.data.data!;
  },

  async getTransaction(id: number): Promise<Transaction> {
    const response = await api.get<ApiResponse<Transaction>>(`/payment/${id}`);
    return response.data.data!;
  },

  async getTransactionDetail(id: number): Promise<TransactionDetail> {
    const response = await api.get<ApiResponse<TransactionDetail>>(`/payment/detail/${id}`);
    return response.data.data!;
  },

  async getTransactionByBillerTrxNo(billerTrxNo: string): Promise<Transaction> {
    const response = await api.get<ApiResponse<Transaction>>(`/payment/status/${billerTrxNo}`);
    return response.data.data!;
  },

  async getHistory(filter: TransactionFilter = {}): Promise<PagedResult<Transaction>> {
    const params = new URLSearchParams();
    Object.entries(filter).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params.append(key, String(value));
      }
    });
    const response = await api.get<ApiResponse<PagedResult<Transaction>>>(`/payment/history?${params}`);
    return response.data.data!;
  },
};

export default paymentService;
