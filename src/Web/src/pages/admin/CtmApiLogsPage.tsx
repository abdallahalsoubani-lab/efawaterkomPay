import { useState, useEffect, useCallback, useRef } from 'react';
import campaignService, { CtmApiLog, CtmApiLogFilter } from '../../services/campaign.service';
import Pagination from '../../components/Pagination';
import usePagination from '../../hooks/usePagination';

function CtmApiLogsPage() {
  const [logs, setLogs] = useState<CtmApiLog[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [selectedLog, setSelectedLog] = useState<CtmApiLog | null>(null);
  const [autoRefresh, setAutoRefresh] = useState(true);
  const [tabValue, setTabValue] = useState<'all' | 'payment-notification' | 'bill-pull'>('all');

  const [filter, setFilter] = useState<CtmApiLogFilter>({
    sortDescending: true,
  });

  const { page, pageSize, goToPage } = usePagination({
    initialPage: 1,
    initialPageSize: 25,
  });

  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const fetchLogs = useCallback(async () => {
    try {
      const endpoint = tabValue === 'all' ? undefined : tabValue;
      const result = await campaignService.getCtmApiLogs({
        ...filter,
        endpoint,
        page,
        pageSize,
      });
      setLogs(result.items);
      setTotalCount(result.totalCount);
    } catch (error) {
      console.error('Failed to fetch CTM API logs:', error);
    } finally {
      setLoading(false);
    }
  }, [filter, page, pageSize, tabValue]);

  useEffect(() => {
    setLoading(true);
    fetchLogs();
  }, [fetchLogs]);

  useEffect(() => {
    if (intervalRef.current) clearInterval(intervalRef.current);
    if (autoRefresh) {
      intervalRef.current = setInterval(fetchLogs, 5000);
    }
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, [autoRefresh, fetchLogs]);

  const handleFilterChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFilter((prev) => ({
      ...prev,
      [name]: value === '' ? undefined : value,
    }));
    goToPage(1);
  };

  const formatDate = (dateString: string) =>
    new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    });

  const getStatusBadgeClass = (status: number) => {
    if (status >= 200 && status < 300) return 'badge-success';
    if (status >= 400 && status < 500) return 'badge-pending';
    if (status >= 500) return 'badge-failed';
    return 'badge-processing';
  };

  const getEndpointBadgeClass = (endpoint: string) => {
    if (endpoint === 'bill-pull') return 'badge-processing';
    if (endpoint === 'payment-notification') return 'badge-success';
    return 'badge-pending';
  };

  const tryFormatJson = (raw?: string): string => {
    if (!raw) return '(empty)';
    try {
      return JSON.stringify(JSON.parse(raw), null, 2);
    } catch {
      return raw;
    }
  };

  return (
    <div className="ctm-api-logs-page">
      <div className="flex-between mb-2">
        <h1>CTM API Logs</h1>
        <div className="flex gap-1">
          <button
            className={`btn btn-sm ${autoRefresh ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => setAutoRefresh(!autoRefresh)}
          >
            Auto Refresh: {autoRefresh ? 'ON' : 'OFF'}
          </button>
          <button className="btn btn-sm btn-secondary" onClick={fetchLogs}>
            Refresh
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 mb-2">
        {(['all', 'bill-pull', 'payment-notification'] as const).map((tab) => (
          <button
            key={tab}
            className={`btn btn-sm ${tabValue === tab ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => { setTabValue(tab); goToPage(1); }}
          >
            {tab === 'all' ? 'All Logs' : tab === 'bill-pull' ? 'Bill Pull' : 'Payment Notifications'}
          </button>
        ))}
      </div>

      <div className="card">
        {/* Filters */}
        <div className="filters mb-2">
          <div className="flex gap-2" style={{ flexWrap: 'wrap' }}>
            <input
              type="text"
              name="billingNo"
              className="form-input"
              style={{ width: '180px' }}
              placeholder="Billing No"
              value={filter.billingNo || ''}
              onChange={handleFilterChange}
            />
            <input
              type="text"
              name="joebppsTrx"
              className="form-input"
              style={{ width: '200px' }}
              placeholder="JOEBPPSTrx"
              value={filter.joebppsTrx || ''}
              onChange={handleFilterChange}
            />
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
          </div>
        </div>

        {loading ? (
          <div className="loading-screen" style={{ minHeight: '200px' }}>
            <div className="spinner"></div>
          </div>
        ) : logs.length === 0 ? (
          <p className="text-center" style={{ padding: '2rem', color: '#666' }}>
            No CTM API logs found
          </p>
        ) : (
          <>
            <div className="table-container">
              <table>
                <thead>
                  <tr>
                    <th>Time</th>
                    <th>Endpoint</th>
                    <th>Status</th>
                    <th>Billing No</th>
                    <th>JOEBPPSTrx</th>
                    <th>Client IP</th>
                    <th>Response Time</th>
                    <th>Error</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {logs.map((log) => (
                    <tr key={log.id}>
                      <td style={{ whiteSpace: 'nowrap', fontSize: '0.85rem' }}>
                        {formatDate(log.timestamp)}
                      </td>
                      <td>
                        <span className={`badge ${getEndpointBadgeClass(log.endpoint)}`}>
                          {log.endpoint}
                        </span>
                      </td>
                      <td>
                        <span className={`badge ${getStatusBadgeClass(log.httpStatusCode)}`}>
                          {log.httpStatusCode || 'pending'}
                        </span>
                      </td>
                      <td>
                        <span style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>
                          {log.billingNo || '-'}
                        </span>
                      </td>
                      <td>
                        <span style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>
                          {log.joebppsTrx || '-'}
                        </span>
                      </td>
                      <td style={{ fontSize: '0.85rem' }}>{log.clientIp || '-'}</td>
                      <td>
                        <span style={{
                          fontFamily: 'monospace',
                          color: log.responseTimeMs > 1000 ? 'var(--error-color)' :
                                 log.responseTimeMs > 500 ? 'var(--warning-color)' : 'var(--success-color)'
                        }}>
                          {log.responseTimeMs}ms
                        </span>
                      </td>
                      <td>
                        {log.errorMessage && (
                          <span style={{ color: 'var(--error-color)', fontSize: '0.8rem' }}
                                title={log.errorMessage}>
                            {log.errorMessage.length > 30
                              ? log.errorMessage.substring(0, 30) + '...'
                              : log.errorMessage}
                          </span>
                        )}
                      </td>
                      <td>
                        <button
                          className="btn btn-sm btn-secondary"
                          onClick={() => setSelectedLog(log)}
                        >
                          View
                        </button>
                      </td>
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

      {/* Detail Modal */}
      {selectedLog && (
        <div className="modal-overlay" onClick={() => setSelectedLog(null)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="flex-between mb-2">
              <h2>Log #{selectedLog.id}</h2>
              <button className="btn btn-sm btn-secondary" onClick={() => setSelectedLog(null)}>
                Close
              </button>
            </div>

            <div className="grid grid-2 mb-2" style={{ gap: '0.5rem' }}>
              <div>
                <strong>Endpoint:</strong>{' '}
                <span className={`badge ${getEndpointBadgeClass(selectedLog.endpoint)}`}>
                  {selectedLog.endpoint}
                </span>
              </div>
              <div>
                <strong>Status:</strong>{' '}
                <span className={`badge ${getStatusBadgeClass(selectedLog.httpStatusCode)}`}>
                  {selectedLog.httpStatusCode}
                </span>
              </div>
              <div><strong>Timestamp:</strong> {formatDate(selectedLog.timestamp)}</div>
              <div><strong>Response Time:</strong> {selectedLog.responseTimeMs}ms</div>
              <div><strong>Client IP:</strong> {selectedLog.clientIp || '-'}</div>
              <div><strong>Billing No:</strong> {selectedLog.billingNo || '-'}</div>
              <div><strong>JOEBPPSTrx:</strong> {selectedLog.joebppsTrx || '-'}</div>
              <div><strong>User Agent:</strong> <span style={{ fontSize: '0.8rem' }}>{selectedLog.userAgent || '-'}</span></div>
            </div>

            {selectedLog.errorMessage && (
              <div className="alert alert-error mb-2">
                <strong>Error:</strong> {selectedLog.errorMessage}
              </div>
            )}

            <div className="mb-2">
              <h3 style={{ marginBottom: '0.5rem' }}>Request Body</h3>
              <pre style={{
                background: '#f5f5f5',
                padding: '1rem',
                borderRadius: 'var(--radius)',
                overflow: 'auto',
                maxHeight: '300px',
                fontSize: '0.8rem',
                lineHeight: '1.4',
                border: '1px solid var(--border-color)',
              }}>
                {tryFormatJson(selectedLog.requestBody)}
              </pre>
            </div>

            <div>
              <h3 style={{ marginBottom: '0.5rem' }}>Response Body</h3>
              <pre style={{
                background: '#f5f5f5',
                padding: '1rem',
                borderRadius: 'var(--radius)',
                overflow: 'auto',
                maxHeight: '300px',
                fontSize: '0.8rem',
                lineHeight: '1.4',
                border: '1px solid var(--border-color)',
              }}>
                {tryFormatJson(selectedLog.responseBody)}
              </pre>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default CtmApiLogsPage;
