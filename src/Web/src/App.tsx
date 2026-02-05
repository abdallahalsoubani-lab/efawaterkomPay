import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './context/AuthContext';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import AdminRoute from './components/AdminRoute';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashboardPage';
import PaymentPage from './pages/PaymentPage';
import PaymentResultPage from './pages/PaymentResultPage';
import HistoryPage from './pages/HistoryPage';
import AdminDashboardPage from './pages/AdminDashboardPage';
import AdminTransactionsPage from './pages/AdminTransactionsPage';
import AdminUsersPage from './pages/AdminUsersPage';
import CampaignsPage from './pages/admin/CampaignsPage';
import CampaignDetailPage from './pages/admin/CampaignDetailPage';
import NewCampaignPage from './pages/admin/NewCampaignPage';
import DonationsPage from './pages/admin/DonationsPage';
import CampaignsPublicPage from './pages/CampaignsPublicPage';

function App() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="loading-screen">
        <div className="spinner"></div>
        <p>Loading...</p>
      </div>
    );
  }

  return (
    <Routes>
      <Route path="/login" element={
        isAuthenticated ? <Navigate to="/dashboard" replace /> : <LoginPage />
      } />
      <Route path="/register" element={
        isAuthenticated ? <Navigate to="/dashboard" replace /> : <RegisterPage />
      } />
      <Route path="/payment/result" element={<PaymentResultPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<Layout />}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/payment/new" element={<PaymentPage />} />
          <Route path="/history" element={<HistoryPage />} />
          <Route path="/campaigns" element={<CampaignsPublicPage />} />

          <Route element={<AdminRoute />}>
            <Route path="/admin/dashboard" element={<AdminDashboardPage />} />
            <Route path="/admin/transactions" element={<AdminTransactionsPage />} />
            <Route path="/admin/users" element={<AdminUsersPage />} />
            <Route path="/admin/campaigns" element={<CampaignsPage />} />
            <Route path="/admin/campaigns/new" element={<NewCampaignPage />} />
            <Route path="/admin/campaigns/:id" element={<CampaignDetailPage />} />
            <Route path="/admin/donations" element={<DonationsPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="/" element={<Navigate to={isAuthenticated ? "/dashboard" : "/login"} replace />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
