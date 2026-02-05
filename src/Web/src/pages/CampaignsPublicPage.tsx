import { useState, useEffect } from 'react';
import campaignService, { Campaign } from '../services/campaign.service';

function CampaignsPublicPage() {
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchCampaigns = async () => {
      try {
        const result = await campaignService.getCampaigns({
          status: 'Active',
          pageSize: 50,
          sortBy: 'CreatedAt',
          sortDescending: true,
        });
        setCampaigns(result.items);
      } catch (error) {
        console.error('Failed to fetch campaigns:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchCampaigns();
  }, []);

  const formatAmount = (amount: number) =>
    new Intl.NumberFormat('en-JO', { style: 'currency', currency: 'JOD' }).format(amount);

  if (loading) {
    return (
      <div className="loading-screen" style={{ minHeight: '300px' }}>
        <div className="spinner"></div>
      </div>
    );
  }

  return (
    <div className="campaigns-public-page">
      <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
        <h1>Active Campaigns</h1>
        <p style={{ color: '#666' }}>Support a cause you care about</p>
      </div>

      {campaigns.length === 0 ? (
        <p className="text-center" style={{ color: '#666' }}>No active campaigns at the moment</p>
      ) : (
        <div className="grid grid-3">
          {campaigns.map((campaign) => {
            const progress = campaign.targetAmount > 0
              ? Math.min(Math.round((campaign.collectedAmount / campaign.targetAmount) * 100), 100)
              : 0;

            return (
              <div key={campaign.id} className="card" style={{ display: 'flex', flexDirection: 'column' }}>
                <div style={{ flex: 1 }}>
                  <span className="badge badge-success" style={{ marginBottom: '0.75rem' }}>
                    {campaign.category}
                  </span>
                  <h3 style={{ margin: '0.5rem 0' }}>{campaign.nameEn}</h3>
                  <p style={{ color: '#666', fontSize: '0.9rem', direction: 'rtl' }}>
                    {campaign.nameAr}
                  </p>
                  {campaign.descriptionEn && (
                    <p style={{ color: '#666', fontSize: '0.85rem', marginTop: '0.5rem' }}>
                      {campaign.descriptionEn}
                    </p>
                  )}
                </div>

                <div style={{ marginTop: '1rem' }}>
                  <div className="flex-between" style={{ fontSize: '0.85rem', marginBottom: '4px' }}>
                    <span style={{ fontWeight: 600 }}>
                      {formatAmount(campaign.collectedAmount)}
                    </span>
                    <span style={{ color: '#666' }}>{progress}%</span>
                  </div>
                  <div style={{
                    background: '#e0e0e0',
                    borderRadius: '4px',
                    height: '8px',
                  }}>
                    <div style={{
                      background: progress >= 100 ? 'var(--success-color)' : 'var(--primary-color)',
                      borderRadius: '4px',
                      height: '100%',
                      width: `${progress}%`,
                      transition: 'width 0.3s',
                    }} />
                  </div>
                  <div className="flex-between" style={{ fontSize: '0.8rem', color: '#666', marginTop: '4px' }}>
                    <span>Target: {formatAmount(campaign.targetAmount)}</span>
                    <span>{campaign.donorsCount} donors</span>
                  </div>

                  <div style={{ marginTop: '0.75rem', fontSize: '0.8rem', color: '#999' }}>
                    <p>Campaign Code: <strong>{campaign.campaignCode}</strong></p>
                    <p>Open eFAWATEERcom app and search for this code to donate</p>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

export default CampaignsPublicPage;
