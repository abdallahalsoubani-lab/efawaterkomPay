import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import paymentService, { PaymentInitiateRequest } from '../services/payment.service';
import useForm from '../hooks/useForm';

function PaymentPage() {
  const { user } = useAuth();
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

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
              onChange={(e) => setFieldValue('paymentType', parseInt(e.target.value))}
              disabled={isSubmitting}
            >
              <option value={1}>Postpaid</option>
              <option value={2}>Prepaid</option>
            </select>
          </div>

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
