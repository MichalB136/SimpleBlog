import { useState } from 'react';
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
  const [open, setOpen] = useState(false);

  const tabs = [
    { id: 'home', label: 'Inspiracje', icon: 'house-door', path: '/' },
    { id: 'about', label: 'O mnie', icon: 'person', path: '/about' },
    { id: 'shop', label: 'Kolekcja', icon: 'shop', path: '/shop' },
    { id: 'contact', label: 'Kontakt', icon: 'envelope', path: '/contact' },
  ];

  return (
    <>
      <nav className="navbar bg-light rounded mb-3 p-2">
        <div className="container-fluid d-flex align-items-center">
          <div className="d-flex align-items-center">
            <button className="btn btn-outline-secondary btn-sm me-2 d-md-none" onClick={() => setOpen(!open)} aria-expanded={open} aria-label="Toggle navigation">
              <i className="bi bi-list" style={{ fontSize: '1.25rem' }}></i>
            </button>
            <div className="d-none d-md-block me-3">
              <span className="text-muted me-2">Zalogowany:</span>
              <strong>{user?.username}</strong>
              <span className="badge bg-primary ms-2">{user?.role}</span>
            </div>
          </div>

          <div className="d-flex align-items-center gap-2 ms-auto">
            <div className="d-none d-md-flex align-items-center gap-2 me-2">
              {itemCount > 0 && (
                <div className="position-relative">
                  <i className="bi bi-cart3 fs-5"></i>
                  <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
                    {itemCount}
                  </span>
                </div>
              )}
              {user?.role === 'Admin' && (
                <Link to="/settings" className="btn btn-outline-secondary btn-sm" title="Panel administracyjny">
                  <i className="bi bi-gear-fill"></i>
                </Link>
              )}
              <button className="btn btn-outline-secondary btn-sm" onClick={() => { logout(); onLogout(); }}>
                <i className="bi bi-box-arrow-right me-1"></i>Wyloguj
              </button>
            </div>
          </div>
        </div>

        <div className={`mt-2 ${open ? 'd-block' : 'd-none'} d-md-block`}> 
          <ul className="nav nav-pills flex-column flex-md-row gap-2 mb-0">
            {tabs.map((tab) => (
              <li key={tab.id} className="nav-item position-relative">
                <Link to={tab.path} className={`nav-link ${location.pathname === tab.path ? 'active' : ''}`} onClick={() => setOpen(false)}>
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
        </div>
      </nav>
    </>
  );
}
