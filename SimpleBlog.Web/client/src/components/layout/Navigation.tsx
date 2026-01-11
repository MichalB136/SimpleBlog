import { useAuth } from '@/context/AuthContext';
import { useCart } from '@/hooks/useCart';

interface NavigationProps {
  activeTab: string;
  onTabChange: (tab: string) => void;
  onLogout: () => void;
}

export function Navigation({ activeTab, onTabChange, onLogout }: NavigationProps) {
  const { user, logout } = useAuth();
  const { itemCount } = useCart();

  const tabs = [
    { id: 'home', label: 'Home', icon: 'house-door' },
    { id: 'about', label: 'O mnie', icon: 'person' },
    { id: 'shop', label: 'Sklep', icon: 'shop' },
    { id: 'contact', label: 'Kontakt', icon: 'envelope' },
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
            <button
              className={`nav-link ${activeTab === tab.id ? 'active' : ''}`}
              onClick={() => onTabChange(tab.id)}
            >
              <i className={`bi bi-${tab.icon} me-2`}></i>
              {tab.label}
              {tab.id === 'shop' && itemCount > 0 && (
                <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger" style={{ fontSize: '0.7rem' }}>
                  {itemCount}
                </span>
              )}
            </button>
          </li>
        ))}
      </ul>
    </>
  );
}
