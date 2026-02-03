import { useState, useEffect } from 'react';
import adminService, { DashboardStatistics } from '../services/admin.service';

function AdminDashboardPage() {
  const [stats, setStats] = useState<DashboardStatistics | null>(null);
  const [loading, setLoading] = useState(true);
  const [dateRange, setDateRange] = useState({
    fromDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    toDate: new Date().toISOString().split('T')[0],
  });

  useEffect(() => {
    const fetchStats = async () => {
      setLoading(true);
      try {
        const data = await adminService.getDashboard(dateRange.fromDate, dateRange.toDate);
        setStats(data);
      } catch (error) {
        console.error('Failed to fetch statistics:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, [dateRange]);

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-JO', {
      style: 'currency',
      currency: 'JOD',
    }).format(amount);
  };

  if (loading) {
    return (
      <div className="loading-screen">
        <div className="spinner"></div>
        <p>Loading statistics...</p>
      </div>
    );
  }

  return (
    <div className="admin-dashboard-page">
      <div className="page-header">
        <h1>Admin Dashboard</h1>
        <div className="flex gap-2">
          <input
            type="date"
            className="form-input"
            style={{ width: 'auto' }}
            value={dateRange.fromDate}
            onChange={(e) => setDateRange((prev) => ({ ...prev, fromDate: e.target.value }))}
          />
          <input
            type="date"
            className="form-input"
            style={{ width: 'auto' }}
            value={dateRange.toDate}
            onChange={(e) => setDateRange((prev) => ({ ...prev, toDate: e.target.value }))}
          />
        </div>
      </div>

      <div className="grid grid-4 mb-4">
        <div className="stat-card info">
          <h3>Total Transactions</h3>
          <div className="value">{stats?.totalTransactions || 0}</div>
        </div>
        <div className="stat-card success">
          <h3>Successful</h3>
          <div className="value">{stats?.successfulTransactions || 0}</div>
        </div>
        <div className="stat-card warning">
          <h3>Pending</h3>
          <div className="value">{stats?.pendingTransactions || 0}</div>
        </div>
        <div className="stat-card error">
          <h3>Failed</h3>
          <div className="value">{stats?.failedTransactions || 0}</div>
        </div>
      </div>

      <div className="grid grid-2 mb-4">
        <div className="stat-card">
          <h3>Total Volume</h3>
          <div className="value" style={{ color: '#1976d2' }}>
            {formatAmount(stats?.totalAmount || 0)}
          </div>
        </div>
        <div className="stat-card success">
          <h3>Successful Volume</h3>
          <div className="value">{formatAmount(stats?.successfulAmount || 0)}</div>
        </div>
      </div>

      <div className="grid grid-2 mb-4">
        <div className="stat-card">
          <h3>Total Users</h3>
          <div className="value">{stats?.totalUsers || 0}</div>
        </div>
        <div className="stat-card success">
          <h3>Active Users</h3>
          <div className="value">{stats?.activeUsers || 0}</div>
        </div>
      </div>

      {stats?.statusBreakdown && stats.statusBreakdown.length > 0 && (
        <div className="card mb-4">
          <h2 className="mb-2">Status Breakdown</h2>
          <div className="status-breakdown">
            {stats.statusBreakdown.map((item) => (
              <div key={item.status} className="breakdown-item">
                <div className="breakdown-header">
                  <span className="status-name">{item.status}</span>
                  <span className="status-count">{item.count}</span>
                </div>
                <div className="breakdown-bar">
                  <div
                    className={`breakdown-fill ${item.status.toLowerCase()}`}
                    style={{ width: `${item.percentage}%` }}
                  />
                </div>
                <span className="breakdown-percentage">{item.percentage}%</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {stats?.dailyStatistics && stats.dailyStatistics.length > 0 && (
        <div className="card">
          <h2 className="mb-2">Daily Statistics</h2>
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Transactions</th>
                  <th>Successful</th>
                  <th>Failed</th>
                  <th>Volume</th>
                </tr>
              </thead>
              <tbody>
                {stats.dailyStatistics.map((day) => (
                  <tr key={day.date}>
                    <td>{new Date(day.date).toLocaleDateString()}</td>
                    <td>{day.transactionCount}</td>
                    <td style={{ color: '#2e7d32' }}>{day.successCount}</td>
                    <td style={{ color: '#d32f2f' }}>{day.failedCount}</td>
                    <td>{formatAmount(day.totalAmount)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      <style>{`
        .status-breakdown {
          display: flex;
          flex-direction: column;
          gap: 1rem;
        }
        .breakdown-item {
          display: flex;
          align-items: center;
          gap: 1rem;
        }
        .breakdown-header {
          width: 150px;
          display: flex;
          justify-content: space-between;
        }
        .status-name {
          font-weight: 500;
        }
        .status-count {
          color: #666;
        }
        .breakdown-bar {
          flex: 1;
          height: 8px;
          background: #f5f5f5;
          border-radius: 4px;
          overflow: hidden;
        }
        .breakdown-fill {
          height: 100%;
          border-radius: 4px;
          transition: width 0.3s ease;
        }
        .breakdown-fill.success { background: #2e7d32; }
        .breakdown-fill.failed { background: #d32f2f; }
        .breakdown-fill.pending { background: #ed6c02; }
        .breakdown-fill.processing { background: #0288d1; }
        .breakdown-fill.cancelled { background: #9e9e9e; }
        .breakdown-percentage {
          width: 50px;
          text-align: right;
          color: #666;
        }
      `}</style>
    </div>
  );
}

export default AdminDashboardPage;
