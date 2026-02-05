import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import campaignService, { Campaign, CampaignFilter } from '../../services/campaign.service';
import Pagination from '../../components/Pagination';
import usePagination from '../../hooks/usePagination';

function CampaignsPage() {
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<CampaignFilter>({
    sortBy: 'CreatedAt',
    sortDescending: true,
  });

  const { page, pageSize, goToPage } = usePagination({
    initialPage: 1,
    initialPageSize: 20,
  });

  const fetchCampaigns = useCallback(async () => {
    setLoading(true);
    try {
      const result = await campaignService.getCampaigns({ ...filter, page, pageSize });
      setCampaigns(result.items);
      setTotalCount(result.totalCount);
    } catch (error) {
      console.error('Failed to fetch campaigns:', error);
    } finally {
      setLoading(false);
    }
  }, [filter, page, pageSize]);

  useEffect(() => {
    fetchCampaigns();
  }, [fetchCampaigns]);

  const handleFilterChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFilter((prev) => ({ ...prev, [name]: value || undefined }));
    goToPage(1);
  };

  const formatAmount = (amount: number) =>
    new Intl.NumberFormat('en-JO', { style: 'currency', currency: 'JOD' }).format(amount);

  const formatDate = (dateString: string) =>
    new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });

  const getStatusClass = (status: string) => {
    switch (status) {
      case 'Active': return 'badge-success';
      case 'Upcoming': return 'badge-processing';
      case 'Closed': return 'badge-failed';
      default: return '';
    }
  };

  return (
    <div className="campaigns-page">
      <div className="flex-between mb-2">
        <h1>Campaigns</h1>
        <Link to="/admin/campaigns/new" className="btn btn-primary">
          + New Campaign
        </Link>
      </div>

      <div className="card">
        <div className="filters mb-2">
          <div className="flex gap-2" style={{ flexWrap: 'wrap' }}>
            <input
              type="text"
              name="searchTerm"
              className="form-input"
              style={{ width: '250px' }}
              placeholder="Search campaigns..."
              value={filter.searchTerm || ''}
              onChange={handleFilterChange}
            />
            <select
              name="status"
              className="form-select"
              style={{ width: 'auto' }}
              value={filter.status || ''}
              onChange={handleFilterChange}
            >
              <option value="">All Statuses</option>
              <option value="Active">Active</option>
              <option value="Upcoming">Upcoming</option>
              <option value="Closed">Closed</option>
            </select>
          </div>
        </div>

        {loading ? (
          <div className="loading-screen" style={{ minHeight: '200px' }}>
            <div className="spinner"></div>
          </div>
        ) : campaigns.length === 0 ? (
          <p className="text-center" style={{ padding: '2rem', color: '#666' }}>
            No campaigns found
          </p>
        ) : (
          <>
            <div className="table-container">
              <table>
                <thead>
                  <tr>
                    <th>Campaign</th>
                    <th>Code</th>
                    <th>Status</th>
                    <th>Progress</th>
                    <th>Donors</th>
                    <th>Period</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {campaigns.map((campaign) => (
                    <tr key={campaign.id}>
                      <td>
                        <span style={{ fontWeight: 500 }}>{campaign.nameEn}</span>
                        <small style={{ display: 'block', color: '#666' }}>
                          {campaign.nameAr}
                        </small>
                        <small style={{ display: 'block', color: '#999' }}>
                          {campaign.category}
                        </small>
                      </td>
                      <td style={{ fontFamily: 'monospace' }}>{campaign.campaignCode}</td>
                      <td>
                        <span className={`badge ${getStatusClass(campaign.status)}`}>
                          {campaign.status}
                        </span>
                      </td>
                      <td>
                        <div style={{ minWidth: '150px' }}>
                          <div className="flex-between" style={{ fontSize: '0.85rem' }}>
                            <span>{formatAmount(campaign.collectedAmount)}</span>
                            <span style={{ color: '#666' }}>
                              {campaign.progressPercentage}%
                            </span>
                          </div>
                          <div
                            style={{
                              background: '#e0e0e0',
                              borderRadius: '4px',
                              height: '6px',
                              marginTop: '4px',
                            }}
                          >
                            <div
                              style={{
                                background: campaign.progressPercentage >= 100
                                  ? 'var(--success-color)'
                                  : 'var(--primary-color)',
                                borderRadius: '4px',
                                height: '100%',
                                width: `${Math.min(campaign.progressPercentage, 100)}%`,
                                transition: 'width 0.3s',
                              }}
                            />
                          </div>
                          <small style={{ color: '#666' }}>
                            of {formatAmount(campaign.targetAmount)}
                          </small>
                        </div>
                      </td>
                      <td>{campaign.donorsCount}</td>
                      <td>
                        <small>
                          {formatDate(campaign.startDate)} -<br />
                          {formatDate(campaign.endDate)}
                        </small>
                      </td>
                      <td>
                        <Link
                          to={`/admin/campaigns/${campaign.id}`}
                          className="btn btn-sm btn-secondary"
                        >
                          View
                        </Link>
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
    </div>
  );
}

export default CampaignsPage;
