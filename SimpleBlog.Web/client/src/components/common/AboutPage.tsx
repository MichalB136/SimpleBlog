import { useState } from 'react';
import { useAbout } from '@/hooks/useAbout';
import { useAuth } from '@/context/AuthContext';

export function AboutPage() {
  const { about, loading, error, update, uploadImage, deleteImage, setError } = useAbout();
  const [editMode, setEditMode] = useState(false);
  const [content, setContent] = useState('');
  const [uploadingImage, setUploadingImage] = useState(false);
  const [imageError, setImageError] = useState('');
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

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setImageError('');
    setUploadingImage(true);
    const result = await uploadImage(file);
    setUploadingImage(false);
    
    if (!result.success) {
      setImageError(result.error || 'Błąd przy przesyłaniu zdjęcia');
    }
  };

  const handleDeleteImage = async () => {
    if (!confirm('Czy na pewno chcesz usunąć zdjęcie?')) return;
    
    setImageError('');
    setUploadingImage(true);
    const result = await deleteImage();
    setUploadingImage(false);
    
    if (!result.success) {
      setImageError(result.error || 'Błąd przy usuwaniu zdjęcia');
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
            {imageError && <div className="alert alert-danger mb-3">{imageError}</div>}
            <div className="mb-4 p-3 bg-light rounded">
              <label className="form-label fw-bold">Zdjęcie</label>
              <div className="mb-3">
                {about?.imageUrl && (
                  <div className="mb-3">
                    <img
                      src={about.imageUrl}
                      alt="Aktualne zdjęcie"
                      className="img-fluid rounded"
                      style={{ maxHeight: '240px', objectFit: 'cover' }}
                    />
                    <button
                      className="btn btn-sm btn-danger mt-2"
                      onClick={handleDeleteImage}
                      disabled={uploadingImage}
                    >
                      <i className="bi bi-trash me-2"></i>
                      Usuń zdjęcie
                    </button>
                  </div>
                )}
              </div>
              <div className="input-group">
                <input
                  type="file"
                  className="form-control"
                  accept="image/*"
                  onChange={handleImageUpload}
                  disabled={uploadingImage}
                />
                {uploadingImage && (
                  <span className="input-group-text">
                    <span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                  </span>
                )}
              </div>
              <small className="text-muted d-block mt-2">
                Obsługiwane formaty: JPEG, PNG, GIF, WebP (maks. 10 MB)
              </small>
            </div>

            <div className="mb-3">
              <label className="form-label fw-bold">Treść</label>
              <textarea
                className="form-control"
                rows={10}
                value={content}
                onChange={(e) => setContent(e.target.value)}
                placeholder="Wpisz treść sekcji 'O mnie'"
              ></textarea>
            </div>

            <div className="d-flex gap-2">
              <button 
                className="btn btn-primary" 
                onClick={handleSave}
                disabled={uploadingImage}
              >
                <i className="bi bi-check me-2"></i>
                Zapisz
              </button>
              <button 
                className="btn btn-secondary" 
                onClick={() => setEditMode(false)}
                disabled={uploadingImage}
              >
                <i className="bi bi-x me-2"></i>
                Anuluj
              </button>
            </div>
          </>
        ) : (
          <>
            <div className="row g-4 align-items-center">
              {/* Image column - left side */}
              {about?.imageUrl && (
                <div className="col-lg-5">
                  <img
                    src={about.imageUrl}
                    alt="O mnie"
                    className="img-fluid rounded"
                    style={{ maxHeight: '380px', objectFit: 'cover', width: '100%' }}
                  />
                </div>
              )}
              {/* Text column - right side */}
              <div className={about?.imageUrl ? 'col-lg-7' : 'col-12'}>
                <p className="lead mb-4">{about?.content || 'Brak treści.'}</p>
                {isAdmin && (
                  <button
                    className="btn btn-outline-primary"
                    onClick={() => {
                      setContent(about?.content || '');
                      setEditMode(true);
                      setImageError('');
                    }}
                  >
                    <i className="bi bi-pencil me-2"></i>
                    Edytuj
                  </button>
                )}
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
