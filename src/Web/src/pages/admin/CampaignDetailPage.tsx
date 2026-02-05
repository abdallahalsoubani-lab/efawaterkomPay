import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import campaignService, { CampaignDetail } from '../../services/campaign.service';

function CampaignDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [campaign, setCampaign] = useState<CampaignDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [editForm, setEditForm] = useState<any>({});
  const [saveError, setSaveError] = useState('');
  const [saveSuccess, setSaveSuccess] = useState(false);

  useEffect(() => {
    if (id) {
      fetchCampaign(parseInt(id));
    }
  }, [id]);

  const fetchCampaign = async (campaignId: number) => {
    setLoading(true);
    try {
      const result = await campaignService.getCampaign(campaignId);
      setCampaign(result);
      setEditForm({
        nameAr: result.nameAr,
        nameEn: result.nameEn,
        descriptionAr: result.descriptionAr || '',
        descriptionEn: result.descriptionEn || '',
        status: result.status,
        targetAmount: result.targetAmount,
        minAmount: result.minAmount,
        maxAmount: result.maxAmount,
        custName: result.custName || '',
        freeText: result.freeText || '',
        email: result.email || '',
        phone: result.phone || '',
      });
    } catch (error) {
      console.error('Failed to fetch campaign:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    if (!id) return;
    setSaveError('');
    setSaveSuccess(false);
    try {
      await campaignService.updateCampaign(parseInt(id), editForm);
      setSaveSuccess(true);
      setEditing(false);
      fetchCampaign(parseInt(id));
      setTimeout(() => setSaveSuccess(false), 3000);
    } catch (err: any) {
      setSaveError(err.response?.data?.error?.message || 'Failed to update campaign');
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

  if (loading) {
    return (
      <div className="loading-screen" style={{ minHeight: '300px' }}>
        <div className="spinner"></div>
      </div>
    );
  }

  if (!campaign) {
    return (
      <div className="card text-center" style={{ padding: '2rem' }}>
        <p>Campaign not found</p>
        <Link to="/admin/campaigns" className="btn btn-primary mt-2">
          Back to Campaigns
        </Link>
      </div>
    );
  }

  const progressPercent = campaign.targetAmount > 0
    ? Math.round((campaign.collectedAmount / campaign.targetAmount) * 100 * 100) / 100
    : 0;

  return (
    <div className="campaign-detail-page">
      <div className="flex-between mb-2">
        <div>
          <Link to="/admin/campaigns" style={{ color: '#666', fontSize: '0.9rem' }}>
            &larr; Back to Campaigns
          </Link>
          <h1 style={{ marginTop: '0.5rem' }}>{campaign.nameEn}</h1>
          <p style={{ color: '#666' }}>{campaign.nameAr}</p>
        </div>
        <div className="flex gap-1">
          <span className={`badge ${campaign.status === 'Active' ? 'badge-success' : campaign.status === 'Upcoming' ? 'badge-processing' : 'badge-failed'}`}>
            {campaign.status}
          </span>
          {!editing && (
            <button className="btn btn-secondary btn-sm" onClick={() => setEditing(true)}>
              Edit
            </button>
          )}
        </div>
      </div>

      {saveSuccess && <div className="alert alert-success">Campaign updated successfully</div>}
      {saveError && <div className="alert alert-error">{saveError}</div>}

      <div className="grid grid-4 mb-3">
        <div className="stat-card success">
          <h3>Collected</h3>
          <div className="value">{formatAmount(campaign.collectedAmount)}</div>
        </div>
        <div className="stat-card info">
          <h3>Target</h3>
          <div className="value">{formatAmount(campaign.targetAmount)}</div>
        </div>
        <div className="stat-card warning">
          <h3>Progress</h3>
          <div className="value">{progressPercent}%</div>
        </div>
        <div className="stat-card">
          <h3>Donors</h3>
          <div className="value">{campaign.donorsCount}</div>
        </div>
      </div>

      {editing ? (
        <div className="card mb-3">
          <h2 style={{ marginBottom: '1rem' }}>Edit Campaign</h2>
          <div className="grid grid-2">
            <div className="form-group">
              <label className="form-label">Name (English)</label>
              <input type="text" className="form-input" value={editForm.nameEn}
                onChange={(e) => setEditForm({ ...editForm, nameEn: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Name (Arabic)</label>
              <input type="text" className="form-input" value={editForm.nameAr} dir="rtl"
                onChange={(e) => setEditForm({ ...editForm, nameAr: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Status</label>
              <select className="form-select" value={editForm.status}
                onChange={(e) => setEditForm({ ...editForm, status: e.target.value })}>
                <option value="Active">Active</option>
                <option value="Upcoming">Upcoming</option>
                <option value="Closed">Closed</option>
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Target Amount (JOD)</label>
              <input type="number" step="0.001" className="form-input" value={editForm.targetAmount}
                onChange={(e) => setEditForm({ ...editForm, targetAmount: parseFloat(e.target.value) })} />
            </div>
            <div className="form-group">
              <label className="form-label">Min Amount (JOD)</label>
              <input type="number" step="0.001" className="form-input" value={editForm.minAmount}
                onChange={(e) => setEditForm({ ...editForm, minAmount: parseFloat(e.target.value) })} />
            </div>
            <div className="form-group">
              <label className="form-label">Max Amount (JOD)</label>
              <input type="number" step="0.001" className="form-input" value={editForm.maxAmount}
                onChange={(e) => setEditForm({ ...editForm, maxAmount: parseFloat(e.target.value) })} />
            </div>
            <div className="form-group">
              <label className="form-label">Contact Email</label>
              <input type="email" className="form-input" value={editForm.email}
                onChange={(e) => setEditForm({ ...editForm, email: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Contact Phone</label>
              <input type="text" className="form-input" value={editForm.phone}
                onChange={(e) => setEditForm({ ...editForm, phone: e.target.value })} />
            </div>
          </div>
          <div className="flex gap-1 mt-2">
            <button className="btn btn-primary" onClick={handleSave}>Save Changes</button>
            <button className="btn btn-secondary" onClick={() => setEditing(false)}>Cancel</button>
          </div>
        </div>
      ) : (
        <div className="card mb-3">
          <h2 style={{ marginBottom: '1rem' }}>Campaign Details</h2>
          <div className="grid grid-2">
            <div>
              <p><strong>Code:</strong> {campaign.campaignCode}</p>
              <p><strong>Bill No:</strong> {campaign.billNo}</p>
              <p><strong>Category:</strong> {campaign.category}</p>
              <p><strong>Service Type:</strong> {campaign.serviceType}</p>
              <p><strong>Bill Type:</strong> {campaign.billType}</p>
            </div>
            <div>
              <p><strong>IBAN:</strong> {campaign.iban}</p>
              <p><strong>Bank Code:</strong> {campaign.bankCode}</p>
              <p><strong>Period:</strong> {formatDate(campaign.startDate)} - {formatDate(campaign.endDate)}</p>
              <p><strong>Payment Range:</strong> {formatAmount(campaign.minAmount)} - {formatAmount(campaign.maxAmount)}</p>
              <p><strong>Customer Name:</strong> {campaign.custName || '-'}</p>
            </div>
          </div>
          {(campaign.descriptionEn || campaign.descriptionAr) && (
            <div className="mt-2">
              {campaign.descriptionEn && <p><strong>Description:</strong> {campaign.descriptionEn}</p>}
              {campaign.descriptionAr && <p dir="rtl"><strong>الوصف:</strong> {campaign.descriptionAr}</p>}
            </div>
          )}
        </div>
      )}

      <div className="card">
        <h2 style={{ marginBottom: '1rem' }}>Recent Donations</h2>
        {campaign.recentDonations.length === 0 ? (
          <p className="text-center" style={{ padding: '1rem', color: '#666' }}>
            No donations yet
          </p>
        ) : (
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>eFAWATEERcom Trx</th>
                  <th>Amount</th>
                  <th>Status</th>
                  <th>Channel</th>
                  <th>Payer</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                {campaign.recentDonations.map((donation) => (
                  <tr key={donation.id}>
                    <td style={{ fontFamily: 'monospace' }}>
                      {donation.joebppsTrx}
                      {donation.bankTrxId && (
                        <small style={{ display: 'block', color: '#666' }}>
                          Bank: {donation.bankTrxId}
                        </small>
                      )}
                    </td>
                    <td>{formatAmount(donation.paidAmount)}</td>
                    <td>
                      <span className={`badge ${donation.pmtStatus === 'Paid' ? 'badge-success' : 'badge-pending'}`}>
                        {donation.pmtStatus}
                      </span>
                      {donation.isAcknowledged && (
                        <small style={{ display: 'block', color: 'var(--success-color)' }}>Acknowledged</small>
                      )}
                    </td>
                    <td>{donation.accessChannel || '-'}</td>
                    <td>
                      {donation.payerId || '-'}
                      {donation.payerNation && (
                        <small style={{ display: 'block', color: '#666' }}>
                          {donation.payerNation}
                        </small>
                      )}
                    </td>
                    <td>{formatDate(donation.processDate)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        {campaign.recentDonations.length > 0 && (
          <div className="text-center mt-2">
            <Link to={`/admin/donations?campaignId=${campaign.id}`} className="btn btn-sm btn-secondary">
              View All Donations
            </Link>
          </div>
        )}
      </div>
    </div>
  );
}

export default CampaignDetailPage;
