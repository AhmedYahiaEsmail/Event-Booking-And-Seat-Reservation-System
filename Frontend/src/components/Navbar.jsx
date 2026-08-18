import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Navbar() {
  const { isAuthenticated, isAdmin, user, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/');
  }

  return (
    <nav className="flex items-center justify-between border-b border-gray-200 px-6 py-4">
      <Link to="/" className="text-lg font-semibold text-gray-900">
        Event Booking
      </Link>

      <div className="flex items-center gap-4">
        {isAuthenticated ? (
          <>
            <span className="text-sm text-gray-600">{user?.firstName}</span>
            <Link to="/dashboard" className="text-sm font-medium text-gray-700 hover:text-gray-900">
              Dashboard
            </Link>
            {isAdmin && (
              <Link to="/admin" className="text-sm font-medium text-gray-700 hover:text-gray-900">
                Admin
              </Link>
            )}
            <button
              type="button"
              onClick={handleLogout}
              className="rounded-md bg-gray-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-gray-700"
            >
              Logout
            </button>
          </>
        ) : (
          <>
            <Link to="/login" className="text-sm font-medium text-gray-700 hover:text-gray-900">
              Login
            </Link>
            <Link
              to="/register"
              className="rounded-md bg-gray-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-gray-700"
            >
              Register
            </Link>
          </>
        )}
      </div>
    </nav>
  );
}
