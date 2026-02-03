import { useState, useEffect, useCallback } from 'react';
import adminService from '../services/admin.service';
import { Transaction, TransactionFilter } from '../services/payment.service';
import TransactionStatusBadge from '../components/TransactionStatusBadge';
import Pagination from '../components/Pagination';
import usePagination from '../hooks/usePagination';

function AdminTransactionsPage() {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);
  const [filter, setFilter] = useState<TransactionFilter>({
    sortBy: 'CreatedAt',
    sortDescending: true,
  });

  const { page, pageSize, goToPage } = usePagination({
    initialPage: 1,
    initialPageSize: 20,
  });

  const fetchTransactions = useCallback(async () => {
    setLoading(true);
    try {
      const result = await adminService.getTransactions({
        ...filter,
        page,
        pageSize,
      });
      setTransactions(result.items);
      setTotalCount(result.totalCount);
    } catch (error) {
      console.error('Failed to fetch transactions:', error);
    } finally {
      setLoading(false);
    }
  }, [filter, page, pageSize]);

  useEffect(() => {
    fetchTransactions();
  }, [fetchTransactions]);

  const handleFilterChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFilter((prev) => ({
      ...prev,
      [name]: value === '' ? undefined : name === 'status' ? parseInt(value) : value,
    }));
    goToPage(1);
  };

  const handleExport = async () => {
    setExporting(true);
    try {
      const blob = await adminService.exportTransactions(
        filter.fromDate,
        filter.toDate,
        filter.status?.toString()
      );
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `transactions_${new Date().toISOString().split('T')[0]}.csv`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (error) {
      console.error('Export failed:', error);
    } finally {
      setExporting(false);
    }
  };

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-JO', {
      style: 'currency',
      currency: 'JOD',
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div className="admin-transactions-page">
      <div className="page-header">
        <h1>All Transactions</h1>
        <button
          className="btn btn-secondary"
          onClick={handleExport}
          disabled={exporting}
        >
          {exporting ? 'Exporting...' : 'Export CSV'}
        </button>
      </div>

      <div className="card">
        <div className="filters mb-2">
          <div className="flex gap-2" style={{ flexWrap: 'wrap' }}>
            <select
              name="status"
              className="form-select"
              style={{ width: 'auto' }}
              value={filter.status ?? ''}
              onChange={handleFilterChange}
            >
              <option value="">All Statuses</option>
              <option value="0">Pending</option>
              <option value="1">Processing</option>
              <option value="2">Success</option>
              <option value="3">Failed</option>
              <option value="4">Cancelled</option>
            </select>
            <input
              type="date"
              name="fromDate"
              className="form-input"
              style={{ width: 'auto' }}
              value={filter.fromDate || ''}
              onChange={handleFilterChange}
            />
            <input
              type="date"
              name="toDate"
              className="form-input"
              style={{ width: 'auto' }}
              value={filter.toDate || ''}
              onChange={handleFilterChange}
            />
            <input
              type="text"
              name="billerTrxNo"
              className="form-input"
              style={{ width: '200px' }}
              placeholder="Transaction ID"
              value={filter.billerTrxNo || ''}
              onChange={handleFilterChange}
            />
          </div>
        </div>

        {loading ? (
          <div className="loading-screen" style={{ minHeight: '200px' }}>
            <div className="spinner"></div>
          </div>
        ) : transactions.length === 0 ? (
          <p className="text-center" style={{ padding: '2rem', color: '#666' }}>
            No transactions found
          </p>
        ) : (
          <>
            <div className="table-container">
              <table>
                <thead>
                  <tr>
                    <th>Transaction ID</th>
                    <th>User</th>
                    <th>Amount</th>
                    <th>Type</th>
                    <th>Billing No</th>
                    <th>Status</th>
                    <th>Date</th>
                  </tr>
                </thead>
                <tbody>
                  {transactions.map((transaction) => (
                    <tr key={transaction.id}>
                      <td>
                        <span style={{ fontFamily: 'monospace' }}>
                          {transaction.billerTrxNo}
                        </span>
                        {transaction.directPayTrxNo && (
                          <small style={{ display: 'block', color: '#666' }}>
                            DP: {transaction.directPayTrxNo}
                          </small>
                        )}
                      </td>
                      <td>
                        <span>{transaction.userFullName}</span>
                        <small style={{ display: 'block', color: '#666' }}>
                          {transaction.userEmail}
                        </small>
                      </td>
                      <td>{formatAmount(transaction.amount)}</td>
                      <td>{transaction.paymentTypeName}</td>
                      <td>{transaction.billingNo || '-'}</td>
                      <td>
                        <TransactionStatusBadge
                          status={transaction.status}
                          statusName={transaction.statusName}
                        />
                        {transaction.trxStatusCode && (
                          <small style={{ display: 'block', color: '#666', marginTop: '4px' }}>
                            Code: {transaction.trxStatusCode}
                          </small>
                        )}
                      </td>
                      <td>{formatDate(transaction.createdAt)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <Pagination
              currentPage={page}
              totalPages={Math.ceil(totalCount / pageSize)}
              onPageChange={goToPage}
            />
          </>
        )}
      </div>
    </div>
  );
}

export default AdminTransactionsPage;
