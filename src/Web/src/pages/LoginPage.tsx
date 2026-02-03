import { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import useForm from '../hooks/useForm';
import './AuthPages.css';

interface LoginFormValues {
  email: string;
  password: string;
}

function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuth();
  const [error, setError] = useState('');

  const from = (location.state as any)?.from?.pathname || '/dashboard';

  const { values, errors, touched, isSubmitting, handleChange, handleBlur, handleSubmit } =
    useForm<LoginFormValues>({
      initialValues: {
        email: '',
        password: '',
      },
      validate: (values) => {
        const errors: Partial<Record<keyof LoginFormValues, string>> = {};
        if (!values.email) {
          errors.email = 'Email is required';
        } else if (!/\S+@\S+\.\S+/.test(values.email)) {
          errors.email = 'Invalid email address';
        }
        if (!values.password) {
          errors.password = 'Password is required';
        }
        return errors;
      },
      onSubmit: async (values) => {
        setError('');
        try {
          await login(values);
          navigate(from, { replace: true });
        } catch (err: any) {
          setError(err.response?.data?.error?.message || err.message || 'Login failed');
        }
      },
    });

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-header">
          <h1>DirectPay Gateway</h1>
          <p>Sign in to your account</p>
        </div>

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={handleSubmit}>
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

          <button type="submit" className="btn btn-primary btn-block" disabled={isSubmitting}>
            {isSubmitting ? 'Signing in...' : 'Sign In'}
          </button>
        </form>

        <p className="auth-footer">
          Don't have an account? <Link to="/register">Register</Link>
        </p>
      </div>
    </div>
  );
}

export default LoginPage;
