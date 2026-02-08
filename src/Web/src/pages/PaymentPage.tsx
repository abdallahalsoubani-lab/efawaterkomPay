import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import paymentService, { PaymentInitiateRequest } from '../services/payment.service';
import campaignService, { Campaign } from '../services/campaign.service';
import useForm from '../hooks/useForm';

function PaymentPage() {
  const { user } = useAuth();
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [selectedCampaign, setSelectedCampaign] = useState<Campaign | null>(null);
  const [loadingCampaigns, setLoadingCampaigns] = useState(true);

  useEffect(() => {
    const fetchCampaigns = async () => {
      try {
        const result = await campaignService.getCampaigns({
          status: 'Active',
          pageSize: 100,
          sortBy: 'nameAr',
        });
        setCampaigns(result.items);
      } catch (err) {
        console.error('Failed to load campaigns:', err);
      } finally {
        setLoadingCampaigns(false);
      }
    };
    fetchCampaigns();
  }, []);

  const { values, errors, touched, isSubmitting, handleChange, handleBlur, handleSubmit, setFieldValue } =
    useForm<PaymentInitiateRequest>({
      initialValues: {
        amount: 0,
        paymentType: 1,
        billingNo: '',
        prepaidCatCode: undefined,
        customerEmail: user?.email || '',
        statementNarrative: '',
        otherDetails: '',
        language: 'AR',
      },
      validate: (values) => {
        const errors: Partial<Record<keyof PaymentInitiateRequest, string>> = {};
        if (!values.amount || values.amount <= 0) {
          errors.amount = 'Amount must be greater than 0';
        }
        if (values.paymentType === 1 && !values.billingNo) {
          errors.billingNo = 'Billing number is required for postpaid payments';
        }
        if (values.paymentType === 2 && !values.prepaidCatCode) {
          errors.prepaidCatCode = 'Prepaid category is required for prepaid payments';
        }
        const invalidCharsRegex = /[~"'&#%]/;
        if (values.statementNarrative && invalidCharsRegex.test(values.statementNarrative)) {
          errors.statementNarrative = 'Special characters (~"\'&#%) are not allowed';
        }
        if (values.billingNo && invalidCharsRegex.test(values.billingNo)) {
          errors.billingNo = 'Special characters (~"\'&#%) are not allowed';
        }
        return errors;
      },
      onSubmit: async (values) => {
        setError('');
        setSuccess(false);
        try {
          const response = await paymentService.initiatePayment(values);
          if (response.success && response.redirectUrl) {
            setSuccess(true);
            setTimeout(() => {
              window.location.href = response.redirectUrl!;
            }, 1500);
          } else {
            setError(response.message || 'Failed to initiate payment');
          }
        } catch (err: any) {
          setError(err.response?.data?.error?.message || err.message || 'Payment initiation failed');
        }
      },
    });

  const handleCampaignSelect = (campaignId: string) => {
    if (!campaignId) {
      setSelectedCampaign(null);
      setFieldValue('billingNo', '');
      setFieldValue('statementNarrative', '');
      return;
    }
    const campaign = campaigns.find((c) => c.id === parseInt(campaignId));
    if (campaign) {
      setSelectedCampaign(campaign);
      setFieldValue('billingNo', campaign.campaignCode);
      setFieldValue('statementNarrative', campaign.nameAr);
    }
  };

  const clearCampaignSelection = () => {
    setSelectedCampaign(null);
    setFieldValue('billingNo', '');
    setFieldValue('statementNarrative', '');
  };

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-JO', {
      style: 'currency',
      currency: 'JOD',
      minimumFractionDigits: 3,
    }).format(amount);
  };

  return (
    <div className="payment-page">
      <div className="page-header">
        <h1>New Payment</h1>
      </div>

      <div className="card" style={{ maxWidth: '600px' }}>
        {error && <div className="alert alert-error">{error}</div>}
        {success && (
          <div className="alert alert-success">
            Payment initiated! Redirecting to eFAWATEERcom...
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label" htmlFor="paymentType">
              Payment Type
            </label>
            <select
              id="paymentType"
              name="paymentType"
              className="form-select"
              value={values.paymentType}
              onChange={(e) => {
                setFieldValue('paymentType', parseInt(e.target.value));
                if (parseInt(e.target.value) === 2) {
                  clearCampaignSelection();
                }
              }}
              disabled={isSubmitting}
            >
              <option value={1}>Postpaid</option>
              <option value={2}>Prepaid</option>
            </select>
          </div>

          {values.paymentType === 1 && (
            <div className="form-group">
              <label className="form-label" htmlFor="campaign">
                Campaign (Optional)
              </label>
              <select
                id="campaign"
                name="campaign"
                className="form-select"
                value={selectedCampaign?.id || ''}
                onChange={(e) => handleCampaignSelect(e.target.value)}
                disabled={isSubmitting || loadingCampaigns}
              >
                <option value="">-- Select a campaign or enter billing number manually --</option>
                {campaigns.map((campaign) => (
                  <option key={campaign.id} value={campaign.id}>
                    {campaign.nameAr} - {campaign.custName || 'N/A'} ({formatAmount(campaign.collectedAmount)} / {formatAmount(campaign.targetAmount)})
                  </option>
                ))}
              </select>
              {loadingCampaigns && (
                <span className="form-hint">Loading campaigns...</span>
              )}
            </div>
          )}

          {selectedCampaign && (
            <div className="campaign-details" style={{
              backgroundColor: '#f8f9fa',
              border: '1px solid #e9ecef',
              borderRadius: '8px',
              padding: '16px',
              marginBottom: '16px',
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '12px' }}>
                <h4 style={{ margin: 0, color: '#495057' }}>{selectedCampaign.nameAr}</h4>
                <button
                  type="button"
                  onClick={clearCampaignSelection}
                  style={{
                    background: 'none',
                    border: 'none',
                    color: '#6c757d',
                    cursor: 'pointer',
                    fontSize: '18px',
                    padding: '0 4px',
                  }}
                  title="Clear selection"
                >
                  ×
                </button>
              </div>
              {selectedCampaign.custName && (
                <p style={{ margin: '0 0 8px 0', color: '#6c757d', fontSize: '14px' }}>
                  <strong>Association:</strong> {selectedCampaign.custName}
                </p>
              )}
              {selectedCampaign.descriptionAr && (
                <p style={{ margin: '0 0 12px 0', color: '#6c757d', fontSize: '14px' }}>
                  {selectedCampaign.descriptionAr.length > 150
                    ? selectedCampaign.descriptionAr.substring(0, 150) + '...'
                    : selectedCampaign.descriptionAr}
                </p>
              )}
              <div style={{ marginBottom: '8px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '13px', marginBottom: '4px' }}>
                  <span>Progress</span>
                  <span>{selectedCampaign.progressPercentage.toFixed(1)}%</span>
                </div>
                <div style={{
                  backgroundColor: '#e9ecef',
                  borderRadius: '4px',
                  height: '8px',
                  overflow: 'hidden',
                }}>
                  <div style={{
                    backgroundColor: selectedCampaign.progressPercentage >= 100 ? '#28a745' : '#007bff',
                    height: '100%',
                    width: `${Math.min(selectedCampaign.progressPercentage, 100)}%`,
                    transition: 'width 0.3s ease',
                  }} />
                </div>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '13px', color: '#6c757d' }}>
                <span>{formatAmount(selectedCampaign.collectedAmount)} collected</span>
                <span>Target: {formatAmount(selectedCampaign.targetAmount)}</span>
              </div>
              {(selectedCampaign.minAmount > 0 || selectedCampaign.maxAmount > 0) && (
                <p style={{ margin: '8px 0 0 0', fontSize: '12px', color: '#868e96' }}>
                  {selectedCampaign.minAmount > 0 && `Min: ${formatAmount(selectedCampaign.minAmount)}`}
                  {selectedCampaign.minAmount > 0 && selectedCampaign.maxAmount > 0 && ' | '}
                  {selectedCampaign.maxAmount > 0 && `Max: ${formatAmount(selectedCampaign.maxAmount)}`}
                </p>
              )}
            </div>
          )}

          <div className="form-group">
            <label className="form-label" htmlFor="amount">
              Amount (JOD)
            </label>
            <input
              id="amount"
              type="number"
              name="amount"
              step="0.001"
              min="0.001"
              className={`form-input ${touched.amount && errors.amount ? 'error' : ''}`}
              value={values.amount || ''}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="Enter amount"
              disabled={isSubmitting}
            />
            {touched.amount && errors.amount && (
              <span className="form-error">{errors.amount}</span>
            )}
          </div>

          {values.paymentType === 1 && (
            <div className="form-group">
              <label className="form-label" htmlFor="billingNo">
                Billing Number
                {selectedCampaign && (
                  <span style={{ fontWeight: 'normal', color: '#6c757d', fontSize: '12px', marginLeft: '8px' }}>
                    (Auto-filled from campaign)
                  </span>
                )}
              </label>
              <input
                id="billingNo"
                type="text"
                name="billingNo"
                maxLength={50}
                className={`form-input ${touched.billingNo && errors.billingNo ? 'error' : ''}`}
                value={values.billingNo || ''}
                onChange={handleChange}
                onBlur={handleBlur}
                placeholder="Enter billing number"
                disabled={isSubmitting}
                readOnly={!!selectedCampaign}
                style={selectedCampaign ? { backgroundColor: '#f8f9fa', cursor: 'not-allowed' } : undefined}
              />
              {touched.billingNo && errors.billingNo && (
                <span className="form-error">{errors.billingNo}</span>
              )}
            </div>
          )}

          {values.paymentType === 2 && (
            <div className="form-group">
              <label className="form-label" htmlFor="prepaidCatCode">
                Prepaid Category Code
              </label>
              <input
                id="prepaidCatCode"
                type="number"
                name="prepaidCatCode"
                className={`form-input ${touched.prepaidCatCode && errors.prepaidCatCode ? 'error' : ''}`}
                value={values.prepaidCatCode || ''}
                onChange={handleChange}
                onBlur={handleBlur}
                placeholder="Enter prepaid category code"
                disabled={isSubmitting}
              />
              {touched.prepaidCatCode && errors.prepaidCatCode && (
                <span className="form-error">{errors.prepaidCatCode}</span>
              )}
            </div>
          )}

          <div className="form-group">
            <label className="form-label" htmlFor="customerEmail">
              Email (Optional)
            </label>
            <input
              id="customerEmail"
              type="email"
              name="customerEmail"
              maxLength={250}
              className="form-input"
              value={values.customerEmail || ''}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="Enter email for receipt"
              disabled={isSubmitting}
            />
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="statementNarrative">
              Description (Optional)
              {selectedCampaign && (
                <span style={{ fontWeight: 'normal', color: '#6c757d', fontSize: '12px', marginLeft: '8px' }}>
                  (Auto-filled from campaign)
                </span>
              )}
            </label>
            <input
              id="statementNarrative"
              type="text"
              name="statementNarrative"
              maxLength={100}
              className={`form-input ${touched.statementNarrative && errors.statementNarrative ? 'error' : ''}`}
              value={values.statementNarrative || ''}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="Payment description"
              disabled={isSubmitting}
            />
            {touched.statementNarrative && errors.statementNarrative && (
              <span className="form-error">{errors.statementNarrative}</span>
            )}
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="language">
              Language
            </label>
            <select
              id="language"
              name="language"
              className="form-select"
              value={values.language}
              onChange={handleChange}
              disabled={isSubmitting}
            >
              <option value="AR">Arabic</option>
              <option value="EN">English</option>
            </select>
          </div>

          <button
            type="submit"
            className="btn btn-primary btn-block"
            disabled={isSubmitting || success}
          >
            {isSubmitting ? 'Processing...' : success ? 'Redirecting...' : 'Proceed to Payment'}
          </button>
        </form>
      </div>
    </div>
  );
}

export default PaymentPage;
