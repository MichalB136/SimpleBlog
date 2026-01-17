import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { useCart } from '@/hooks/useCart';

interface NavigationProps {
  onLogout: () => void;
}

export function Navigation({ onLogout }: Readonly<NavigationProps>) {
  const { user, logout } = useAuth();
  const { itemCount } = useCart();
  const location = useLocation();

  const tabs = [
    { id: 'home', label: 'Home', icon: 'house-door', path: '/' },
    { id: 'about', label: 'O mnie', icon: 'person', path: '/about' },
    { id: 'shop', label: 'Sklep', icon: 'shop', path: '/shop' },
    { id: 'contact', label: 'Kontakt', icon: 'envelope', path: '/contact' },
  ];

  return (
    <>
      <div className="d-flex justify-content-between align-items-center p-3 bg-light rounded mb-4">
        <div>
          <span className="text-muted me-2">Zalogowany:</span>
          <strong>{user?.username} </strong>
          <span className="badge bg-primary">{user?.role}</span>
        </div>
        <div className="d-flex align-items-center gap-2">
          {itemCount > 0 && (
            <div className="position-relative">
              <i className="bi bi-cart3 fs-5"></i>
              <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
                {itemCount}
              </span>
            </div>
          )}
          {user?.role === 'Admin' && (
            <Link
              to="/settings"
              className="btn btn-outline-secondary btn-sm"
              title="Panel administracyjny"
            >
              <i className="bi bi-gear-fill"></i>
            </Link>
          )}
          <button
            className="btn btn-outline-secondary btn-sm"
            onClick={() => {
              logout();
              onLogout();
            }}
          >
            <i className="bi bi-box-arrow-right me-1"></i>Wyloguj
          </button>
        </div>
      </div>

      <ul className="nav nav-tabs mb-4">
        {tabs.map((tab) => (
          <li key={tab.id} className="nav-item position-relative">
            <Link
              to={tab.path}
              className={`nav-link ${location.pathname === tab.path ? 'active' : ''}`}
            >
              <i className={`bi bi-${tab.icon} me-2`}></i>
              {tab.label}
              {tab.id === 'shop' && itemCount > 0 && (
                <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger" style={{ fontSize: '0.7rem' }}>
                  {itemCount}
                </span>
              )}
            </Link>
          </li>
        ))}
      </ul>
    </>
  );
}
