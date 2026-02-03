import { useState, useEffect, useCallback } from 'react';
import adminService, { UserDetail, UserFilter } from '../services/admin.service';
import Pagination from '../components/Pagination';
import usePagination from '../hooks/usePagination';

function AdminUsersPage() {
  const [users, setUsers] = useState<UserDetail[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [editingUser, setEditingUser] = useState<string | null>(null);
  const [filter, setFilter] = useState<UserFilter>({
    sortBy: 'CreatedAt',
    sortDescending: true,
  });

  const { page, pageSize, goToPage } = usePagination({
    initialPage: 1,
    initialPageSize: 20,
  });

  const fetchUsers = useCallback(async () => {
    setLoading(true);
    try {
      const result = await adminService.getUsers({
        ...filter,
        page,
        pageSize,
      });
      setUsers(result.items);
      setTotalCount(result.totalCount);
    } catch (error) {
      console.error('Failed to fetch users:', error);
    } finally {
      setLoading(false);
    }
  }, [filter, page, pageSize]);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  const handleFilterChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFilter((prev) => ({
      ...prev,
      [name]: value === '' ? undefined : name === 'isActive' ? value === 'true' : value,
    }));
    goToPage(1);
  };

  const handleToggleActive = async (userId: string, isActive: boolean) => {
    try {
      await adminService.updateUser(userId, { isActive: !isActive });
      fetchUsers();
    } catch (error) {
      console.error('Failed to update user:', error);
    }
  };

  const handleUpdateRoles = async (userId: string, role: string, add: boolean) => {
    const user = users.find((u) => u.id === userId);
    if (!user) return;

    const newRoles = add
      ? [...user.roles, role]
      : user.roles.filter((r) => r !== role);

    try {
      await adminService.updateUserRoles(userId, newRoles);
      fetchUsers();
    } catch (error) {
      console.error('Failed to update roles:', error);
    }
  };

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-JO', {
      style: 'currency',
      currency: 'JOD',
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  return (
    <div className="admin-users-page">
      <div className="page-header">
        <h1>User Management</h1>
      </div>

      <div className="card">
        <div className="filters mb-2">
          <div className="flex gap-2" style={{ flexWrap: 'wrap' }}>
            <input
              type="text"
              name="searchTerm"
              className="form-input"
              style={{ width: '250px' }}
              placeholder="Search by name or email"
              value={filter.searchTerm || ''}
              onChange={handleFilterChange}
            />
            <select
              name="isActive"
              className="form-select"
              style={{ width: 'auto' }}
              value={filter.isActive?.toString() ?? ''}
              onChange={handleFilterChange}
            >
              <option value="">All Users</option>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
            </select>
            <select
              name="role"
              className="form-select"
              style={{ width: 'auto' }}
              value={filter.role || ''}
              onChange={handleFilterChange}
            >
              <option value="">All Roles</option>
              <option value="Admin">Admin</option>
              <option value="User">User</option>
              <option value="Merchant">Merchant</option>
            </select>
          </div>
        </div>

        {loading ? (
          <div className="loading-screen" style={{ minHeight: '200px' }}>
            <div className="spinner"></div>
          </div>
        ) : users.length === 0 ? (
          <p className="text-center" style={{ padding: '2rem', color: '#666' }}>
            No users found
          </p>
        ) : (
          <>
            <div className="table-container">
              <table>
                <thead>
                  <tr>
                    <th>User</th>
                    <th>Roles</th>
                    <th>Transactions</th>
                    <th>Total Volume</th>
                    <th>Status</th>
                    <th>Registered</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((user) => (
                    <tr key={user.id}>
                      <td>
                        <span style={{ fontWeight: 500 }}>{user.fullName}</span>
                        <small style={{ display: 'block', color: '#666' }}>
                          {user.email}
                        </small>
                        {user.phoneNumber && (
                          <small style={{ display: 'block', color: '#666' }}>
                            {user.phoneNumber}
                          </small>
                        )}
                      </td>
                      <td>
                        {editingUser === user.id ? (
                          <div className="flex gap-1" style={{ flexWrap: 'wrap' }}>
                            {['Admin', 'User', 'Merchant'].map((role) => (
                              <label
                                key={role}
                                style={{
                                  display: 'flex',
                                  alignItems: 'center',
                                  gap: '4px',
                                  fontSize: '0.85rem',
                                }}
                              >
                                <input
                                  type="checkbox"
                                  checked={user.roles.includes(role)}
                                  onChange={(e) =>
                                    handleUpdateRoles(user.id, role, e.target.checked)
                                  }
                                />
                                {role}
                              </label>
                            ))}
                            <button
                              className="btn btn-sm btn-secondary"
                              onClick={() => setEditingUser(null)}
                            >
                              Done
                            </button>
                          </div>
                        ) : (
                          <div className="flex gap-1" style={{ flexWrap: 'wrap' }}>
                            {user.roles.map((role) => (
                              <span
                                key={role}
                                className={`badge ${role === 'Admin' ? 'badge-processing' : 'badge-success'}`}
                              >
                                {role}
                              </span>
                            ))}
                          </div>
                        )}
                      </td>
                      <td>{user.transactionCount}</td>
                      <td>{formatAmount(user.totalTransactionAmount)}</td>
                      <td>
                        <span className={`badge ${user.isActive ? 'badge-success' : 'badge-failed'}`}>
                          {user.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td>
                        {formatDate(user.createdAt)}
                        {user.lastLoginAt && (
                          <small style={{ display: 'block', color: '#666' }}>
                            Last login: {formatDate(user.lastLoginAt)}
                          </small>
                        )}
                      </td>
                      <td>
                        <div className="flex gap-1">
                          {editingUser !== user.id && (
                            <button
                              className="btn btn-sm btn-secondary"
                              onClick={() => setEditingUser(user.id)}
                            >
                              Edit Roles
                            </button>
                          )}
                          <button
                            className={`btn btn-sm ${user.isActive ? 'btn-danger' : 'btn-primary'}`}
                            onClick={() => handleToggleActive(user.id, user.isActive)}
                          >
                            {user.isActive ? 'Deactivate' : 'Activate'}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <Pagination
              currentPage={page}
              totalPages={Math.ceil(totalCount / pageSize)}
              onPageChange={goToPage}
            />
          </>
        )}
      </div>
    </div>
  );
}

export default AdminUsersPage;
