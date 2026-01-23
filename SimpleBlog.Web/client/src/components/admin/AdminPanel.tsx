import React, { useCallback, useEffect, useMemo, useState, type ChangeEvent } from 'react';
import { useSiteSettings } from '@/hooks/useSiteSettings';
import { useAuth } from '@/context/AuthContext';
import { TagManagement } from './TagManagement';
import { ProductManagement } from './ProductManagement';

type AdminTab = 'site' | 'tags' | 'products' | 'analytics';

const Analytics = React.lazy(() => import('./Analytics').then((m) => ({ default: m.Analytics })));

export function AdminPanel({ showTitle = true }: { showTitle?: boolean }) {
  const { user } = useAuth();
  const { settings, availableThemes, loading, updateTheme, uploadLogo, deleteLogo } = useSiteSettings();
  const [selectedTheme, setSelectedTheme] = useState('');
  const [updating, setUpdating] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [activeTab, setActiveTab] = useState<AdminTab>('site');

  useEffect(() => {
    if (settings?.theme) {
      setSelectedTheme(settings.theme);
    }
  }, [settings?.theme]);

  const themeDisplayNameMap = useMemo(
    () => ({
      light: 'Jasny',
      dark: 'Ciemny',
      ocean: 'Ocean',
      forest: 'Las',
      sunset: 'Zachód słońca',
      purple: 'Fioletowy',
      marjan: 'Marjan',
    }),
    [],
  );

  const themeIconMap = useMemo(
    () => ({
      light: 'sun',
      dark: 'moon-stars',
      ocean: 'water',
      forest: 'tree',
      sunset: 'brightness-alt-high',
      purple: 'palette',
      marjan: 'flower1',
    }),
    [],
  );

  if (user?.role !== 'Admin') {
    return (
      <div className="container mt-5">
        <div className="alert alert-danger">
          <i className="bi bi-exclamation-triangle me-2" aria-hidden="true"></i>{' '}
          Dostęp tylko dla administratorów
        </div>
      </div>
    );
  }

  const handleThemeUpdate = useCallback(async () => {
    if (!selectedTheme) return;

    setUpdating(true);
    setMessage(null);

    const result = await updateTheme(selectedTheme);

    if (result.success) {
      setMessage({ type: 'success', text: 'Motyw został pomyślnie zaktualizowany!' });
    } else {
      setMessage({ type: 'error', text: result.error || 'Błąd podczas aktualizacji motywu' });
    }

    setUpdating(false);

    // Clear message after 3 seconds
    setTimeout(() => setMessage(null), 3000);
  }, [selectedTheme, updateTheme]);

  const handleLogoUpload = useCallback(async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith('image/')) {
      setMessage({ type: 'error', text: 'Plik musi być obrazem (JPEG, PNG, GIF, WebP)' });
      return;
    }

    // Validate file size (5MB limit)
    if (file.size > 5 * 1024 * 1024) {
      setMessage({ type: 'error', text: 'Rozmiar pliku nie może przekraczać 5 MB' });
      return;
    }

    setUploading(true);
    setMessage(null);

    const result = await uploadLogo(file);

    if (result.success) {
      setMessage({ type: 'success', text: 'Logo zostało pomyślnie przesłane!' });
    } else {
      setMessage({ type: 'error', text: result.error || 'Błąd podczas przesyłania logo' });
    }

    setUploading(false);

    // Clear file input
    event.target.value = '';

    setTimeout(() => setMessage(null), 3000);
  }, [uploadLogo]);

  const handleLogoDelete = useCallback(async () => {
    if (!globalThis.confirm('Czy na pewno chcesz usunąć logo?')) return;

    setUploading(true);
    setMessage(null);

    const result = await deleteLogo();

    if (result.success) {
      setMessage({ type: 'success', text: 'Logo zostało usunięte' });
    } else {
      setMessage({ type: 'error', text: result.error || 'Błąd podczas usuwania logo' });
    }

    setUploading(false);

    setTimeout(() => setMessage(null), 3000);
  }, [deleteLogo]);

  const getThemeDisplayName = useCallback((theme: string) => themeDisplayNameMap[theme as keyof typeof themeDisplayNameMap] || theme, [themeDisplayNameMap]);
  const getThemeIcon = useCallback((theme: string) => themeIconMap[theme as keyof typeof themeIconMap] || 'palette', [themeIconMap]);

  const renderAlert = () =>
    message && (
      <div className={`alert alert-${message.type === 'success' ? 'success' : 'danger'} alert-dismissible fade show`} role="alert">
        <i className={`bi bi-${message.type === 'success' ? 'check-circle' : 'exclamation-triangle'} me-2`} aria-hidden="true"></i>
        {message.text}
        <button
          type="button"
          className="btn-close"
          onClick={() => setMessage(null)}
          aria-label="Close"
        ></button>
      </div>
    );

  const renderLogoCard = () => (
    <div className="card mb-4">
      <div className="card-header">
        <h5 className="mb-0">
          <i className="bi bi-image me-2" aria-hidden="true"></i>{' '}
          Zarządzanie logo
        </h5>
      </div>
      <div className="card-body">
        {loading ? (
          <div className="text-center py-4" aria-live="polite">
            <div className="spinner-border" aria-hidden="true">
              <span className="visually-hidden">Ładowanie...</span>
            </div>
          </div>
        ) : (
          <>
            <div className="mb-3">
              <div className="form-label fw-bold">Aktualne logo:</div>
              {settings?.logoUrl ? (
                <div className="d-flex align-items-center gap-3">
                  <img
                    src={settings.logoUrl}
                    alt="Logo strony"
                    style={{ maxHeight: '100px', maxWidth: '300px', objectFit: 'contain' }}
                    className="border rounded p-2"
                  />
                  <button
                    className="btn btn-outline-danger btn-sm"
                    onClick={handleLogoDelete}
                    disabled={uploading}
                  >
                    <i className="bi bi-trash me-2" aria-hidden="true"></i>{' '}
                    Usuń logo
                  </button>
                </div>
              ) : (
                <p className="text-muted">Brak logo</p>
              )}
            </div>

            <div className="mb-3">
              <label htmlFor="logoUpload" className="form-label fw-bold">
                Prześlij nowe logo:
              </label>
              <input
                type="file"
                id="logoUpload"
                className="form-control"
                accept="image/*"
                onChange={handleLogoUpload}
                disabled={uploading}
              />
              <small className="text-muted d-block mt-1">
                Maksymalny rozmiar: 5 MB. Dozwolone formaty: JPEG, PNG, GIF, WebP
              </small>
            </div>

            {uploading && (
              <output className="alert alert-info d-block" aria-live="polite">
                <div className="spinner-border spinner-border-sm me-2" aria-hidden="true"></div>
                Przesyłanie logo...
              </output>
            )}
          </>
        )}
      </div>
    </div>
  );

  const renderThemeCard = () => (
    <div className="card">
      <div className="card-header">
        <h5 className="mb-0">
          <i className="bi bi-palette me-2" aria-hidden="true"></i>{' '}
          Zarządzanie motywami
        </h5>
      </div>
      <div className="card-body">
        {loading ? (
          <div className="text-center py-4" aria-live="polite">
            <div className="spinner-border" aria-hidden="true">
              <span className="visually-hidden">Ładowanie...</span>
            </div>
          </div>
        ) : (
          <>
            <div className="mb-3">
              <div className="form-label fw-bold">Aktualny motyw:</div>
              <p className="text-muted">
                {settings ? getThemeDisplayName(settings.theme) : 'Nie ustawiono'}
                {settings && (
                  <small className="d-block mt-1">
                    Ostatnia aktualizacja: {new Date(settings.updatedAt).toLocaleString('pl-PL')} przez {settings.updatedBy}
                  </small>
                )}
              </p>
            </div>

            <div className="mb-4">
              <label htmlFor="themeSelect" className="form-label fw-bold">
                Wybierz motyw:
              </label>
              <select
                id="themeSelect"
                className="form-select"
                value={selectedTheme}
                onChange={(e) => setSelectedTheme(e.target.value)}
                disabled={updating}
              >
                <option value="">-- Wybierz motyw --</option>
                {availableThemes.map((theme) => (
                  <option key={theme} value={theme}>
                    {getThemeDisplayName(theme)}
                  </option>
                ))}
              </select>
            </div>

            <div className="row g-3 mb-4">
              {availableThemes.map((theme) => (
                <div key={theme} className="col-md-4">
                  <button
                    type="button"
                    className={`card h-100 w-100 cursor-pointer border ${selectedTheme === theme ? 'border-primary' : ''}`}
                    onClick={() => setSelectedTheme(theme)}
                    aria-pressed={selectedTheme === theme}
                    style={{ cursor: 'pointer', background: 'transparent' }}
                  >
                    <div className="card-body text-center">
                      <i className={`bi bi-${getThemeIcon(theme)} fs-1 mb-2`} aria-hidden="true"></i>
                      <h6 className="card-title">{getThemeDisplayName(theme)}</h6>
                      {selectedTheme === theme && (
                        <span className="badge bg-primary mt-2">
                          <i className="bi bi-check-circle me-1" aria-hidden="true"></i>{' '}
                          Wybrany
                        </span>
                      )}
                    </div>
                  </button>
                </div>
              ))}
            </div>

            <div className="d-flex gap-2">
              <button
                className="btn btn-primary"
                onClick={handleThemeUpdate}
                disabled={updating || !selectedTheme || selectedTheme === settings?.theme}
              >
                {updating ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>{' '}
                    Aktualizowanie...
                  </>
                ) : (
                  <>
                    <i className="bi bi-save me-2" aria-hidden="true"></i>{' '}
                    Zapisz motyw
                  </>
                )}
              </button>
              {selectedTheme !== settings?.theme && (
                <button
                  className="btn btn-outline-secondary"
                  onClick={() => setSelectedTheme(settings?.theme || '')}
                  disabled={updating}
                >
                  <i className="bi bi-x-circle me-2" aria-hidden="true"></i>{' '}
                  Anuluj
                </button>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  );

  const renderInfoCard = () => (
    <div className="card mt-4">
      <div className="card-header">
        <h5 className="mb-0">
          <i className="bi bi-info-circle me-2" aria-hidden="true"></i>{' '}
          Informacje
        </h5>
      </div>
      <div className="card-body">
        <p className="mb-2">
          <strong>Dostępne motywy:</strong> {availableThemes.length}
        </p>
        <p className="mb-0">
          <small className="text-muted">
            Motyw wybrany w panelu administratora jest stosowany dla wszystkich użytkowników strony.
          </small>
        </p>
      </div>
    </div>
  );

  const renderSiteTab = () => (
    <>
      {renderAlert()}
      {renderLogoCard()}
      {renderThemeCard()}
      {renderInfoCard()}
    </>
  );

  return (
    <div className="container mt-4">
      {showTitle && (
        <h2 className="mb-4">
          <i className="bi bi-gear me-2" aria-hidden="true"></i>{' '}
          Panel Administratora
        </h2>
      )}

      {/* Tabs Navigation */}
      <ul className="nav nav-tabs mb-4" role="tablist">
        <li className="nav-item" role="presentation">
          <button
            className={`nav-link ${activeTab === 'site' ? 'active' : ''}`}
            onClick={() => setActiveTab('site')}
            type="button"
            role="tab"
            aria-selected={activeTab === 'site'}
          >
            <i className="bi bi-palette me-2" aria-hidden="true"></i>{' '}
            Ustawienia strony
          </button>
        </li>
        <li className="nav-item" role="presentation">
          <button
            className={`nav-link ${activeTab === 'tags' ? 'active' : ''}`}
            onClick={() => setActiveTab('tags')}
            type="button"
            role="tab"
            aria-selected={activeTab === 'tags'}
          >
            <i className="bi bi-tags me-2" aria-hidden="true"></i>{' '}
            Zarządzanie tagami
          </button>
        </li>
        <li className="nav-item" role="presentation">
          <button
            className={`nav-link ${activeTab === 'products' ? 'active' : ''}`}
            onClick={() => setActiveTab('products')}
            type="button"
            role="tab"
            aria-selected={activeTab === 'products'}
          >
            <i className="bi bi-bag me-2" aria-hidden="true"></i>{' '}
            Produkty
          </button>
        </li>
        <li className="nav-item" role="presentation">
          <button
            className={`nav-link ${activeTab === 'analytics' ? 'active' : ''}`}
            onClick={() => setActiveTab('analytics')}
            type="button"
            role="tab"
            aria-selected={activeTab === 'analytics'}
          >
            <i className="bi bi-bar-chart me-2" aria-hidden="true"></i>{' '}
            Analityka
          </button>
        </li>
      </ul>

      {/* Tab Content */}
      {activeTab === 'tags' && <TagManagement />}
      {activeTab === 'products' && <ProductManagement />}
      {activeTab === 'analytics' && (
        // Lazy-load Analytics component to keep bundle small
        <React.Suspense fallback={<div>Ładowanie...</div>}>
          <Analytics />
        </React.Suspense>
      )}
      {activeTab === 'site' && renderSiteTab()}
    </div>
  );
}
