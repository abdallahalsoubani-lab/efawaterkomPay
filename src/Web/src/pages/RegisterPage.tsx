import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import useForm from '../hooks/useForm';
import './AuthPages.css';

interface RegisterFormValues {
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
}

function RegisterPage() {
  const navigate = useNavigate();
  const { register } = useAuth();
  const [error, setError] = useState('');

  const { values, errors, touched, isSubmitting, handleChange, handleBlur, handleSubmit } =
    useForm<RegisterFormValues>({
      initialValues: {
        fullName: '',
        email: '',
        phoneNumber: '',
        password: '',
        confirmPassword: '',
      },
      validate: (values) => {
        const errors: Partial<Record<keyof RegisterFormValues, string>> = {};
        if (!values.fullName) {
          errors.fullName = 'Full name is required';
        }
        if (!values.email) {
          errors.email = 'Email is required';
        } else if (!/\S+@\S+\.\S+/.test(values.email)) {
          errors.email = 'Invalid email address';
        }
        if (!values.password) {
          errors.password = 'Password is required';
        } else if (values.password.length < 6) {
          errors.password = 'Password must be at least 6 characters';
        }
        if (values.password !== values.confirmPassword) {
          errors.confirmPassword = 'Passwords do not match';
        }
        return errors;
      },
      onSubmit: async (values) => {
        setError('');
        try {
          await register(values);
          navigate('/dashboard');
        } catch (err: any) {
          setError(err.response?.data?.error?.message || err.message || 'Registration failed');
        }
      },
    });

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-header">
          <h1>DirectPay Gateway</h1>
          <p>Create your account</p>
        </div>

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label" htmlFor="fullName">
              Full Name
            </label>
            <input
              id="fullName"
              type="text"
              name="fullName"
              className={`form-input ${touched.fullName && errors.fullName ? 'error' : ''}`}
              value={values.fullName}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="Enter your full name"
              disabled={isSubmitting}
            />
            {touched.fullName && errors.fullName && (
              <span className="form-error">{errors.fullName}</span>
            )}
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="email">
              Email
            </label>
            <input
              id="email"
              type="email"
              name="email"
              className={`form-input ${touched.email && errors.email ? 'error' : ''}`}
              value={values.email}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="Enter your email"
              disabled={isSubmitting}
            />
            {touched.email && errors.email && (
              <span className="form-error">{errors.email}</span>
            )}
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="phoneNumber">
              Phone Number (Optional)
            </label>
            <input
              id="phoneNumber"
              type="tel"
              name="phoneNumber"
              className="form-input"
              value={values.phoneNumber}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="Enter your phone number"
              disabled={isSubmitting}
            />
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="password">
              Password
            </label>
            <input
              id="password"
              type="password"
              name="password"
              className={`form-input ${touched.password && errors.password ? 'error' : ''}`}
              value={values.password}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="Enter your password"
              disabled={isSubmitting}
            />
            {touched.password && errors.password && (
              <span className="form-error">{errors.password}</span>
            )}
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="confirmPassword">
              Confirm Password
            </label>
            <input
              id="confirmPassword"
              type="password"
              name="confirmPassword"
              className={`form-input ${touched.confirmPassword && errors.confirmPassword ? 'error' : ''}`}
              value={values.confirmPassword}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="Confirm your password"
              disabled={isSubmitting}
            />
            {touched.confirmPassword && errors.confirmPassword && (
              <span className="form-error">{errors.confirmPassword}</span>
            )}
          </div>

          <button type="submit" className="btn btn-primary btn-block" disabled={isSubmitting}>
            {isSubmitting ? 'Creating Account...' : 'Create Account'}
          </button>
        </form>

        <p className="auth-footer">
          Already have an account? <Link to="/login">Sign In</Link>
        </p>
      </div>
    </div>
  );
}

export default RegisterPage;
