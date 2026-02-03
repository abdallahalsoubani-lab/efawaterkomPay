import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import paymentService, { Transaction } from '../services/payment.service';
import TransactionStatusBadge from '../components/TransactionStatusBadge';
import './PaymentResultPage.css';

function PaymentResultPage() {
  const [searchParams] = useSearchParams();
  const [transaction, setTransaction] = useState<Transaction | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const txnId = searchParams.get('txn');
  const errorParam = searchParams.get('error');
  const statusParam = searchParams.get('status');

  useEffect(() => {
    const fetchTransaction = async () => {
      if (errorParam) {
        setError('Payment processing failed. Please try again.');
        setLoading(false);
        return;
      }

      if (!txnId) {
        setError('Invalid transaction reference');
        setLoading(false);
        return;
      }

      try {
        const data = await paymentService.getTransaction(parseInt(txnId));
        setTransaction(data);
      } catch (err: any) {
        setError('Failed to load transaction details');
      } finally {
        setLoading(false);
      }
    };

    fetchTransaction();
  }, [txnId, errorParam]);

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-JO', {
      style: 'currency',
      currency: 'JOD',
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const isSuccess = transaction?.status === 2;
  const isFailed = transaction?.status === 3 || transaction?.status === 4;

  if (loading) {
    return (
      <div className="result-page">
        <div className="result-card">
          <div className="loading-screen">
            <div className="spinner"></div>
            <p>Loading payment result...</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="result-page">
      <div className="result-card">
        {error || isFailed ? (
          <div className="result-header error">
            <div className="result-icon">&#10060;</div>
            <h1>Payment Failed</h1>
            <p>{error || transaction?.trxStatusMessage || 'Your payment could not be processed'}</p>
          </div>
        ) : isSuccess ? (
          <div className="result-header success">
            <div className="result-icon">&#10004;</div>
            <h1>Payment Successful</h1>
            <p>Your payment has been processed successfully</p>
          </div>
        ) : (
          <div className="result-header processing">
            <div className="result-icon">&#8987;</div>
            <h1>Payment Processing</h1>
            <p>Your payment is being processed</p>
          </div>
        )}

        {transaction && (
          <div className="result-details">
            <div className="detail-row">
              <span className="label">Transaction ID</span>
              <span className="value">{transaction.billerTrxNo}</span>
            </div>
            {transaction.directPayTrxNo && (
              <div className="detail-row">
                <span className="label">DirectPay Reference</span>
                <span className="value">{transaction.directPayTrxNo}</span>
              </div>
            )}
            <div className="detail-row">
              <span className="label">Amount</span>
              <span className="value amount">{formatAmount(transaction.amount)}</span>
            </div>
            <div className="detail-row">
              <span className="label">Payment Type</span>
              <span className="value">{transaction.paymentTypeName}</span>
            </div>
            {transaction.billingNo && (
              <div className="detail-row">
                <span className="label">Billing Number</span>
                <span className="value">{transaction.billingNo}</span>
              </div>
            )}
            <div className="detail-row">
              <span className="label">Status</span>
              <span className="value">
                <TransactionStatusBadge
                  status={transaction.status}
                  statusName={transaction.statusName}
                />
              </span>
            </div>
            <div className="detail-row">
              <span className="label">Date</span>
              <span className="value">{formatDate(transaction.createdAt)}</span>
            </div>
            {transaction.trxStatusMessage && (
              <div className="detail-row">
                <span className="label">Message</span>
                <span className="value">{transaction.trxStatusMessage}</span>
              </div>
            )}
          </div>
        )}

        <div className="result-actions">
          <Link to="/payment/new" className="btn btn-primary">
            Make Another Payment
          </Link>
          <Link to="/history" className="btn btn-secondary">
            View History
          </Link>
        </div>
      </div>
    </div>
  );
}

export default PaymentResultPage;
