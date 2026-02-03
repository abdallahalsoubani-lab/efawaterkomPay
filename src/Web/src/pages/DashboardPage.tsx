import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import paymentService, { Transaction } from '../services/payment.service';
import TransactionStatusBadge from '../components/TransactionStatusBadge';

function DashboardPage() {
  const { user } = useAuth();
  const [recentTransactions, setRecentTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState({
    total: 0,
    successful: 0,
    pending: 0,
    failed: 0,
  });

  useEffect(() => {
    const fetchData = async () => {
      try {
        const result = await paymentService.getHistory({ page: 1, pageSize: 5 });
        setRecentTransactions(result.items);

        const allTransactions = await paymentService.getHistory({ page: 1, pageSize: 1000 });
        setStats({
          total: allTransactions.totalCount,
          successful: allTransactions.items.filter((t) => t.status === 2).length,
          pending: allTransactions.items.filter((t) => t.status === 0 || t.status === 1).length,
          failed: allTransactions.items.filter((t) => t.status === 3 || t.status === 4).length,
        });
      } catch (error) {
        console.error('Failed to fetch dashboard data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

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

  if (loading) {
    return (
      <div className="loading-screen">
        <div className="spinner"></div>
        <p>Loading dashboard...</p>
      </div>
    );
  }

  return (
    <div className="dashboard-page">
      <div className="page-header">
        <h1>Welcome, {user?.fullName}</h1>
        <Link to="/payment/new" className="btn btn-primary">
          New Payment
        </Link>
      </div>

      <div className="grid grid-4 mb-4">
        <div className="stat-card info">
          <h3>Total Transactions</h3>
          <div className="value">{stats.total}</div>
        </div>
        <div className="stat-card success">
          <h3>Successful</h3>
          <div className="value">{stats.successful}</div>
        </div>
        <div className="stat-card warning">
          <h3>Pending</h3>
          <div className="value">{stats.pending}</div>
        </div>
        <div className="stat-card error">
          <h3>Failed</h3>
          <div className="value">{stats.failed}</div>
        </div>
      </div>

      <div className="card">
        <div className="flex-between mb-2">
          <h2>Recent Transactions</h2>
          <Link to="/history" className="btn btn-secondary btn-sm">
            View All
          </Link>
        </div>

        {recentTransactions.length === 0 ? (
          <p className="text-center" style={{ padding: '2rem', color: '#666' }}>
            No transactions yet. <Link to="/payment/new">Make your first payment</Link>
          </p>
        ) : (
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Transaction ID</th>
                  <th>Amount</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                {recentTransactions.map((transaction) => (
                  <tr key={transaction.id}>
                    <td>{transaction.billerTrxNo}</td>
                    <td>{formatAmount(transaction.amount)}</td>
                    <td>{transaction.paymentTypeName}</td>
                    <td>
                      <TransactionStatusBadge
                        status={transaction.status}
                        statusName={transaction.statusName}
                      />
                    </td>
                    <td>{formatDate(transaction.createdAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

export default DashboardPage;
