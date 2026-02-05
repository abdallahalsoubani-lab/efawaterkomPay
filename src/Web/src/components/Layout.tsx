import { Outlet, Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Layout.css';

function Layout() {
  const { user, isAdmin, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const isActive = (path: string) => location.pathname === path || location.pathname.startsWith(path + '/');

  return (
    <div className="layout">
      <nav className="sidebar">
        <div className="sidebar-header">
          <h1>DirectPay</h1>
          <span className="subtitle">Gateway</span>
        </div>

        <div className="sidebar-menu">
          <div className="menu-section">
            <span className="menu-label">Main</span>
            <Link to="/dashboard" className={`menu-item ${isActive('/dashboard') ? 'active' : ''}`}>
              <span className="icon">&#128200;</span>
              Dashboard
            </Link>
            <Link to="/payment/new" className={`menu-item ${isActive('/payment/new') ? 'active' : ''}`}>
              <span className="icon">&#128179;</span>
              New Payment
            </Link>
            <Link to="/history" className={`menu-item ${isActive('/history') ? 'active' : ''}`}>
              <span className="icon">&#128203;</span>
              History
            </Link>
            <Link to="/campaigns" className={`menu-item ${isActive('/campaigns') ? 'active' : ''}`}>
              <span className="icon">&#127775;</span>
              Campaigns
            </Link>
          </div>

          {isAdmin && (
            <div className="menu-section">
              <span className="menu-label">Admin</span>
              <Link to="/admin/dashboard" className={`menu-item ${isActive('/admin/dashboard') ? 'active' : ''}`}>
                <span className="icon">&#128202;</span>
                Statistics
              </Link>
              <Link to="/admin/transactions" className={`menu-item ${isActive('/admin/transactions') ? 'active' : ''}`}>
                <span className="icon">&#128176;</span>
                Transactions
              </Link>
              <Link to="/admin/users" className={`menu-item ${isActive('/admin/users') ? 'active' : ''}`}>
                <span className="icon">&#128101;</span>
                Users
              </Link>
            </div>
          )}

          {isAdmin && (
            <div className="menu-section">
              <span className="menu-label">CTM / Donations</span>
              <Link to="/admin/campaigns" className={`menu-item ${isActive('/admin/campaigns') ? 'active' : ''}`}>
                <span className="icon">&#127919;</span>
                Campaigns
              </Link>
              <Link to="/admin/donations" className={`menu-item ${isActive('/admin/donations') ? 'active' : ''}`}>
                <span className="icon">&#128156;</span>
                Donations
              </Link>
            </div>
          )}
        </div>

        <div className="sidebar-footer">
          <div className="user-info">
            <span className="user-avatar">{user?.fullName?.charAt(0).toUpperCase()}</span>
            <div className="user-details">
              <span className="user-name">{user?.fullName}</span>
              <span className="user-email">{user?.email}</span>
            </div>
          </div>
          <button className="logout-btn" onClick={handleLogout}>
            Logout
          </button>
        </div>
      </nav>

      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}

export default Layout;
