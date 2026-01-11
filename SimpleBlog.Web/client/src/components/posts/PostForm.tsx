import React, { useState } from 'react';
// Unused type import removed

interface PostFormProps {
  onCreated?: () => void;
  isSubmitting?: boolean;
}

export function PostForm({ onCreated, isSubmitting = false }: PostFormProps) {
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [author, setAuthor] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [error, setError] = useState('');

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (event) => {
        setImageUrl((event.target?.result as string) || '');
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim() || !content.trim()) {
      setError('Title and content are required');
      return;
    }
    onCreated?.();
  };

  return (
    <div className="card shadow mb-4">
      <div className="card-header bg-primary text-white d-flex justify-content-between align-items-center">
        <div>
          <small className="text-uppercase d-block mb-1">Nowy wpis</small>
          <h5 className="mb-0">Dodaj notkę</h5>
        </div>
        <button type="submit" form="post-form" className="btn btn-light btn-sm" disabled={isSubmitting}>
          {isSubmitting ? 'Zapisywanie...' : (
            <>
              <i className="bi bi-send me-1"></i>Publikuj
            </>
          )}
        </button>
      </div>
      <div className="card-body">
        <form id="post-form" onSubmit={handleSubmit}>
          <div className="mb-3">
            <label className="form-label">Tytuł</label>
            <input
              className="form-control"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Mój pierwszy wpis"
              required
            />
          </div>
          <div className="mb-3">
            <label className="form-label">Autor</label>
            <input
              className="form-control"
              value={author}
              onChange={(e) => setAuthor(e.target.value)}
              placeholder="Twoje imię"
            />
          </div>
          <div className="mb-3">
            <label className="form-label">Treść</label>
            <textarea
              className="form-control"
              rows={5}
              value={content}
              onChange={(e) => setContent(e.target.value)}
              placeholder="Co masz dziś do powiedzenia?"
              required
            ></textarea>
          </div>
          <div className="mb-3">
            <label className="form-label">Zdjęcie (opcjonalnie)</label>
            <input
              type="file"
              className="form-control"
              accept="image/*"
              onChange={handleImageChange}
            />
            {imageUrl && (
              <img src={imageUrl} className="img-fluid rounded mt-2" style={{ maxHeight: '200px' }} alt="Preview" />
            )}
          </div>
          {error && <div className="alert alert-danger">{error}</div>}
        </form>
      </div>
    </div>
  );
}
