import { useState } from 'react';
import { useAbout } from '@/hooks/useAbout';
import { useAuth } from '@/context/AuthContext';

export function AboutPage() {
  const { about, loading, error, update, setError } = useAbout();
  const [editMode, setEditMode] = useState(false);
  const [content, setContent] = useState('');
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';

  const handleSave = async () => {
    try {
      await update(content);
      setEditMode(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save');
    }
  };

  if (loading) return <p className="text-muted">Ładowanie...</p>;

  return (
    <div className="card shadow-sm">
      <div className="card-body p-5">
        <h2 className="mb-4">O mnie</h2>
        {error && <div className="alert alert-danger">{error}</div>}
        {editMode && isAdmin ? (
          <>
            <textarea
              className="form-control mb-3"
              rows={10}
              value={content}
              onChange={(e) => setContent(e.target.value)}
              placeholder="Wpisz treść sekcji 'O mnie'"
            ></textarea>
            <div className="d-flex gap-2">
              <button className="btn btn-primary" onClick={handleSave}>
                Zapisz
              </button>
              <button className="btn btn-secondary" onClick={() => setEditMode(false)}>
                Anuluj
              </button>
            </div>
          </>
        ) : (
          <>
            <p className="lead mb-4">{about?.content || 'Brak treści.'}</p>
            {isAdmin && (
              <button
                className="btn btn-outline-primary"
                onClick={() => {
                  setContent(about?.content || '');
                  setEditMode(true);
                }}
              >
                Edytuj
              </button>
            )}
          </>
        )}
      </div>
    </div>
  );
}
