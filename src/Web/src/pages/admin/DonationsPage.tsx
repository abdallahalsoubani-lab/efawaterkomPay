import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import campaignService, { Donation, DonationFilter } from '../../services/campaign.service';
import Pagination from '../../components/Pagination';
import usePagination from '../../hooks/usePagination';

function DonationsPage() {
  const [searchParams] = useSearchParams();
  const [donations, setDonations] = useState<Donation[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  const initialCampaignId = searchParams.get('campaignId');
  const [filter, setFilter] = useState<DonationFilter>({
    campaignId: initialCampaignId ? parseInt(initialCampaignId) : undefined,
    sortBy: 'CreatedAt',
    sortDescending: true,
  });

  const { page, pageSize, goToPage } = usePagination({
    initialPage: 1,
    initialPageSize: 20,
  });

  const fetchDonations = useCallback(async () => {
    setLoading(true);
    try {
      const result = await campaignService.getDonations({ ...filter, page, pageSize });
      setDonations(result.items);
      setTotalCount(result.totalCount);
    } catch (error) {
      console.error('Failed to fetch donations:', error);
    } finally {
      setLoading(false);
    }
  }, [filter, page, pageSize]);

  useEffect(() => {
    fetchDonations();
  }, [fetchDonations]);

  const handleFilterChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;
    setFilter((prev) => ({
      ...prev,
      [name]: value === '' ? undefined : type === 'number' ? parseFloat(value) : value,
    }));
    goToPage(1);
  };

  const handleExport = async () => {
    setExporting(true);
    try {
      const blob = await campaignService.exportDonations(
        filter.fromDate,
        filter.toDate,
        filter.campaignId
      );
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `donations_${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Failed to export:', error);
    } finally {
      setExporting(false);
    }
  };

  const formatAmount = (amount: number) =>
    new Intl.NumberFormat('en-JO', { style: 'currency', currency: 'JOD' }).format(amount);

  const formatDate = (dateString: string) =>
    new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });

  return (
    <div className="donations-page">
      <div className="flex-between mb-2">
        <h1>Donations</h1>
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
            <input
              type="text"
              name="joebppsTrx"
              className="form-input"
              style={{ width: '200px' }}
              placeholder="eFAWATEERcom Trx ID"
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
            <input
              type="number"
              name="minAmount"
              className="form-input"
              style={{ width: '120px' }}
              placeholder="Min amount"
              step="0.001"
              value={filter.minAmount || ''}
              onChange={handleFilterChange}
            />
            <input
              type="number"
              name="maxAmount"
              className="form-input"
              style={{ width: '120px' }}
              placeholder="Max amount"
              step="0.001"
              value={filter.maxAmount || ''}
              onChange={handleFilterChange}
            />
          </div>
        </div>

        {loading ? (
          <div className="loading-screen" style={{ minHeight: '200px' }}>
            <div className="spinner"></div>
          </div>
        ) : donations.length === 0 ? (
          <p className="text-center" style={{ padding: '2rem', color: '#666' }}>
            No donations found
          </p>
        ) : (
          <>
            <div className="table-container">
              <table>
                <thead>
                  <tr>
                    <th>eFAWATEERcom Trx</th>
                    <th>Campaign</th>
                    <th>Amount</th>
                    <th>Status</th>
                    <th>Channel</th>
                    <th>Payer</th>
                    <th>Date</th>
                  </tr>
                </thead>
                <tbody>
                  {donations.map((d) => (
                    <tr key={d.id}>
                      <td>
                        <span style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>
                          {d.joebppsTrx}
                        </span>
                        {d.bankTrxId && (
                          <small style={{ display: 'block', color: '#666' }}>
                            Bank: {d.bankTrxId}
                          </small>
                        )}
                      </td>
                      <td>
                        {d.campaignName || d.billingNo}
                        {d.campaignCode && (
                          <small style={{ display: 'block', color: '#666' }}>
                            {d.campaignCode}
                          </small>
                        )}
                      </td>
                      <td>
                        {formatAmount(d.paidAmount)}
                        {d.feesAmount > 0 && (
                          <small style={{ display: 'block', color: '#666' }}>
                            Fees: {formatAmount(d.feesAmount)}
                          </small>
                        )}
                      </td>
                      <td>
                        <span className={`badge ${d.pmtStatus === 'Paid' ? 'badge-success' : 'badge-pending'}`}>
                          {d.pmtStatus}
                        </span>
                        {d.isAcknowledged && (
                          <small style={{ display: 'block', color: 'var(--success-color)' }}>
                            ACK
                          </small>
                        )}
                      </td>
                      <td>
                        {d.accessChannel || '-'}
                        {d.paymentMethod && (
                          <small style={{ display: 'block', color: '#666' }}>
                            {d.paymentMethod}
                          </small>
                        )}
                      </td>
                      <td>
                        {d.payerId || '-'}
                        {d.payerNation && (
                          <small style={{ display: 'block', color: '#666' }}>
                            {d.payerNation}
                          </small>
                        )}
                      </td>
                      <td>{formatDate(d.processDate)}</td>
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

export default DonationsPage;
