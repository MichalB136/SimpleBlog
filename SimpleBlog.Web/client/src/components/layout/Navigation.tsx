import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { useCart } from '@/hooks/useCart';
import { useSiteSettings } from '@/hooks/useSiteSettings';
import { buildResponsiveProps } from '@/utils/image';

interface NavigationProps {
  onLogout: () => void;
}

export function Navigation({ onLogout }: Readonly<NavigationProps>) {
  const { user, logout } = useAuth();
  const { itemCount } = useCart();
  const { settings } = useSiteSettings();
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
        <div className="container-fluid d-flex align-items-start position-relative">
          <div className="d-flex align-items-center">
            <button className="btn btn-outline-secondary btn-sm me-2 d-md-none" onClick={() => setOpen(!open)} aria-expanded={open} aria-label="Toggle navigation">
              <i className="bi bi-list" style={{ fontSize: '1.25rem' }}></i>
            </button>
          </div>

          {settings?.logoUrl && (
            <div className="position-absolute top-50 start-50 translate-middle">
              <LogoImage src={`${settings.logoUrl}?t=${settings.updatedAt}`} />
            </div>
          )}

          <div className="d-none d-md-flex flex-column align-items-end gap-2 ms-auto">
            <div className="d-none d-md-block">
              <span className="text-muted me-2">Zalogowany:</span>
              <strong>{user?.username}</strong>
              <span className="badge bg-primary ms-2">{user?.role}</span>
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
                <Link to="/settings" className="btn btn-outline-secondary btn-sm" title="Panel administracyjny">
                  <i className="bi bi-gear-fill"></i>
                </Link>
              )}
              <button className="btn btn-outline-secondary btn-sm" onClick={() => { logout(); onLogout(); }}>
                <i className="bi bi-box-arrow-right me-1"></i>Wyloguj
              </button>
            </div>

            <div className={`mt-2 ${open ? 'd-block' : 'd-none'} d-md-block`}> 
              <ul className="nav nav-pills flex-column gap-2 mb-0">
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
          </div>
        </div>
      </nav>
    </>
  );
}

function LogoImage({ src }: { src: string }) {
  const { src: s, srcSet, sizes } = buildResponsiveProps(src);
  return (
    <img
      src={s}
      srcSet={srcSet}
      sizes={sizes}
      alt="Logo"
      style={{
        maxHeight: '220px',
        maxWidth: '220px',
        objectFit: 'contain',
        borderRadius: '50%',
        padding: '20px',
        backgroundColor: '#ffffff',
        boxShadow: '0 6px 12px rgba(0, 0, 0, 0.12), 0 2px 4px rgba(0, 0, 0, 0.08)'
      }}
      className="img-fluid"
    />
  );
}
