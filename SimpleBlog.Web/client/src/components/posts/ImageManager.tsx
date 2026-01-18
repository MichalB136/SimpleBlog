import React, { useState } from 'react';
import type { Post } from '@/types/post';

interface ImageManagerProps {
  post: Post;
  onAddImage: (file: File) => Promise<void>;
  onRemoveImage: (imageUrl: string) => Promise<void>;
}

export function ImageManager({ post, onAddImage, onRemoveImage }: ImageManagerProps) {
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith('image/')) {
      setError('Plik musi być obrazem (JPEG, PNG, GIF, WebP)');
      return;
    }

    // Validate file size (10MB limit)
    if (file.size > 10 * 1024 * 1024) {
      setError('Rozmiar pliku nie może przekraczać 10 MB');
      return;
    }

    setUploading(true);
    setError('');

    try {
      await onAddImage(file);
      // Clear file input
      e.target.value = '';
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Błąd podczas przesyłania zdjęcia');
    } finally {
      setUploading(false);
    }
  };

  const handleRemove = async (imageUrl: string) => {
    if (!window.confirm('Czy na pewno chcesz usunąć to zdjęcie?')) return;

    try {
      await onRemoveImage(imageUrl);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Błąd podczas usuwania zdjęcia');
    }
  };

  return (
    <div className="border rounded p-3 mb-3">
      <h6 className="mb-3">
        <i className="bi bi-images me-2"></i>
        Zdjęcia ({post.imageUrls.length})
      </h6>
      
      {error && (
        <div className="alert alert-danger alert-dismissible" role="alert">
          {error}
          <button
            type="button"
            className="btn-close"
            onClick={() => setError('')}
          ></button>
        </div>
      )}

      {post.imageUrls.length > 0 && (
        <div className="row g-2 mb-3">
          {post.imageUrls.map((url, index) => (
            <div key={index} className="col-md-4">
              <div className="card">
                <img
                  src={url}
                  className="card-img-top"
                  style={{ height: '150px', objectFit: 'cover' }}
                  alt={`Zdjęcie ${index + 1}`}
                />
                <div className="card-body p-2">
                  <button
                    className="btn btn-danger btn-sm w-100"
                    onClick={() => handleRemove(url)}
                    disabled={uploading}
                  >
                    <i className="bi bi-trash me-1"></i>
                    Usuń
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="mb-0">
        <label className="form-label mb-2">Dodaj nowe zdjęcie</label>
        <input
          type="file"
          className="form-control"
          accept="image/*"
          onChange={handleFileSelect}
          disabled={uploading}
        />
        {uploading && (
          <div className="text-muted small mt-2">
            <span className="spinner-border spinner-border-sm me-2"></span>
            Przesyłanie...
          </div>
        )}
        <small className="text-muted d-block mt-1">
          Dozwolone formaty: JPEG, PNG, GIF, WebP. Maksymalny rozmiar: 10 MB
        </small>
      </div>
    </div>
  );
}
