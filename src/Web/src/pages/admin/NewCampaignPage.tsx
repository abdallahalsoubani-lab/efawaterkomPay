import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import campaignService, { CreateCampaignRequest } from '../../services/campaign.service';

function NewCampaignPage() {
  const navigate = useNavigate();
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState<CreateCampaignRequest>({
    campaignCode: '',
    billNo: '',
    nameAr: '',
    nameEn: '',
    descriptionAr: '',
    descriptionEn: '',
    category: 'Donations',
    serviceType: 'Donations',
    status: 'Active',
    billType: 'Recurring',
    billCustomerCat: 'Citizen',
    targetAmount: 0,
    startDate: new Date().toISOString().slice(0, 16),
    endDate: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString().slice(0, 16),
    allowPartialPayment: true,
    minAmount: 1.0,
    maxAmount: 100000.0,
    iban: '',
    bankCode: '',
    custName: '',
    freeText: '',
    email: '',
    phone: '',
  });

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => {
    const { name, value, type } = e.target;
    setForm((prev) => ({
      ...prev,
      [name]: type === 'number' ? parseFloat(value) || 0 :
              type === 'checkbox' ? (e.target as HTMLInputElement).checked : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);

    try {
      const result = await campaignService.createCampaign(form);
      navigate(`/admin/campaigns/${result.id}`);
    } catch (err: any) {
      setError(err.response?.data?.error?.message || 'Failed to create campaign');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="new-campaign-page">
      <Link to="/admin/campaigns" style={{ color: '#666', fontSize: '0.9rem' }}>
        &larr; Back to Campaigns
      </Link>
      <h1 style={{ marginTop: '0.5rem', marginBottom: '1.5rem' }}>Create New Campaign</h1>

      {error && <div className="alert alert-error">{error}</div>}

      <form onSubmit={handleSubmit}>
        <div className="card mb-3">
          <h2 style={{ marginBottom: '1rem' }}>Basic Information</h2>
          <div className="grid grid-2">
            <div className="form-group">
              <label className="form-label">Campaign Code *</label>
              <input type="text" name="campaignCode" className="form-input"
                value={form.campaignCode} onChange={handleChange} required maxLength={50}
                pattern="^[a-zA-Z0-9]+$" title="Alphanumeric only" />
            </div>
            <div className="form-group">
              <label className="form-label">Bill No *</label>
              <input type="text" name="billNo" className="form-input"
                value={form.billNo} onChange={handleChange} required maxLength={50} />
            </div>
            <div className="form-group">
              <label className="form-label">Name (English) *</label>
              <input type="text" name="nameEn" className="form-input"
                value={form.nameEn} onChange={handleChange} required maxLength={200} />
            </div>
            <div className="form-group">
              <label className="form-label">Name (Arabic) *</label>
              <input type="text" name="nameAr" className="form-input" dir="rtl"
                value={form.nameAr} onChange={handleChange} required maxLength={200} />
            </div>
            <div className="form-group">
              <label className="form-label">Description (English)</label>
              <textarea name="descriptionEn" className="form-input"
                value={form.descriptionEn} onChange={handleChange} maxLength={1000}
                rows={3} style={{ resize: 'vertical' }} />
            </div>
            <div className="form-group">
              <label className="form-label">Description (Arabic)</label>
              <textarea name="descriptionAr" className="form-input" dir="rtl"
                value={form.descriptionAr} onChange={handleChange} maxLength={1000}
                rows={3} style={{ resize: 'vertical' }} />
            </div>
            <div className="form-group">
              <label className="form-label">Category</label>
              <input type="text" name="category" className="form-input"
                value={form.category} onChange={handleChange} />
            </div>
            <div className="form-group">
              <label className="form-label">Status</label>
              <select name="status" className="form-select" value={form.status} onChange={handleChange}>
                <option value="Active">Active</option>
                <option value="Upcoming">Upcoming</option>
                <option value="Closed">Closed</option>
              </select>
            </div>
          </div>
        </div>

        <div className="card mb-3">
          <h2 style={{ marginBottom: '1rem' }}>Financial Settings</h2>
          <div className="grid grid-2">
            <div className="form-group">
              <label className="form-label">Target Amount (JOD) *</label>
              <input type="number" name="targetAmount" step="0.001" className="form-input"
                value={form.targetAmount} onChange={handleChange} required min={0} />
            </div>
            <div className="form-group">
              <label className="form-label">IBAN *</label>
              <input type="text" name="iban" className="form-input"
                value={form.iban} onChange={handleChange} required maxLength={50}
                placeholder="JO94CBJO0010000000000131000302" />
            </div>
            <div className="form-group">
              <label className="form-label">Bank Code *</label>
              <input type="text" name="bankCode" className="form-input"
                value={form.bankCode} onChange={handleChange} required maxLength={10} />
            </div>
            <div className="form-group">
              <label className="form-label">Min Donation (JOD)</label>
              <input type="number" name="minAmount" step="0.001" className="form-input"
                value={form.minAmount} onChange={handleChange} min={0.001} />
            </div>
            <div className="form-group">
              <label className="form-label">Max Donation (JOD)</label>
              <input type="number" name="maxAmount" step="0.001" className="form-input"
                value={form.maxAmount} onChange={handleChange} min={0.001} />
            </div>
          </div>
        </div>

        <div className="card mb-3">
          <h2 style={{ marginBottom: '1rem' }}>Period & Contact</h2>
          <div className="grid grid-2">
            <div className="form-group">
              <label className="form-label">Start Date *</label>
              <input type="datetime-local" name="startDate" className="form-input"
                value={form.startDate} onChange={handleChange} required />
            </div>
            <div className="form-group">
              <label className="form-label">End Date *</label>
              <input type="datetime-local" name="endDate" className="form-input"
                value={form.endDate} onChange={handleChange} required />
            </div>
            <div className="form-group">
              <label className="form-label">Customer Name</label>
              <input type="text" name="custName" className="form-input"
                value={form.custName} onChange={handleChange} maxLength={200} />
            </div>
            <div className="form-group">
              <label className="form-label">Free Text</label>
              <input type="text" name="freeText" className="form-input"
                value={form.freeText} onChange={handleChange} maxLength={500} />
            </div>
            <div className="form-group">
              <label className="form-label">Email</label>
              <input type="email" name="email" className="form-input"
                value={form.email} onChange={handleChange} maxLength={250} />
            </div>
            <div className="form-group">
              <label className="form-label">Phone</label>
              <input type="text" name="phone" className="form-input"
                value={form.phone} onChange={handleChange} maxLength={20} />
            </div>
          </div>
        </div>

        <div className="flex gap-1">
          <button type="submit" className="btn btn-primary" disabled={submitting}>
            {submitting ? 'Creating...' : 'Create Campaign'}
          </button>
          <Link to="/admin/campaigns" className="btn btn-secondary">Cancel</Link>
        </div>
      </form>
    </div>
  );
}

export default NewCampaignPage;
