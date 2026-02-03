import api, { ApiResponse, PagedResult } from './api';
import { Transaction, TransactionFilter } from './payment.service';

export interface DashboardStatistics {
  totalTransactions: number;
  successfulTransactions: number;
  failedTransactions: number;
  pendingTransactions: number;
  totalAmount: number;
  successfulAmount: number;
  totalUsers: number;
  activeUsers: number;
  dailyStatistics: DailyStatistic[];
  statusBreakdown: StatusBreakdown[];
}

export interface DailyStatistic {
  date: string;
  transactionCount: number;
  totalAmount: number;
  successCount: number;
  failedCount: number;
}

export interface StatusBreakdown {
  status: string;
  count: number;
  percentage: number;
}

export interface UserDetail {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
  roles: string[];
  transactionCount: number;
  totalTransactionAmount: number;
}

export interface UserFilter {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  isActive?: boolean;
  role?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface UpdateUserRequest {
  fullName?: string;
  phoneNumber?: string;
  isActive?: boolean;
}

export const adminService = {
  async getDashboard(fromDate?: string, toDate?: string): Promise<DashboardStatistics> {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    const response = await api.get<ApiResponse<DashboardStatistics>>(`/admin/dashboard?${params}`);
    return response.data.data!;
  },

  async getTransactions(filter: TransactionFilter = {}): Promise<PagedResult<Transaction>> {
    const params = new URLSearchParams();
    Object.entries(filter).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params.append(key, String(value));
      }
    });
    const response = await api.get<ApiResponse<PagedResult<Transaction>>>(`/admin/transactions?${params}`);
    return response.data.data!;
  },

  async exportTransactions(fromDate?: string, toDate?: string, status?: string): Promise<Blob> {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    if (status) params.append('status', status);
    params.append('format', 'csv');
    const response = await api.get(`/admin/transactions/export?${params}`, {
      responseType: 'blob',
    });
    return response.data;
  },

  async getUsers(filter: UserFilter = {}): Promise<PagedResult<UserDetail>> {
    const params = new URLSearchParams();
    Object.entries(filter).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params.append(key, String(value));
      }
    });
    const response = await api.get<ApiResponse<PagedResult<UserDetail>>>(`/admin/users?${params}`);
    return response.data.data!;
  },

  async getUser(id: string): Promise<UserDetail> {
    const response = await api.get<ApiResponse<UserDetail>>(`/admin/users/${id}`);
    return response.data.data!;
  },

  async updateUser(id: string, data: UpdateUserRequest): Promise<void> {
    await api.put(`/admin/users/${id}`, data);
  },

  async updateUserRoles(id: string, roles: string[]): Promise<void> {
    await api.put(`/admin/users/${id}/roles`, { roles });
  },
};

export default adminService;
