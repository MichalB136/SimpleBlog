import { useState, useEffect } from 'react';
import { tagsApi, type Tag, type CreateTagRequest, type UpdateTagRequest } from '../../api/tags';

export function TagManagement() {
  const [tags, setTags] = useState<Tag[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editingTag, setEditingTag] = useState<Tag | null>(null);
  const [showForm, setShowForm] = useState(false);

  useEffect(() => {
    loadTags();
  }, []);

  const loadTags = async () => {
    try {
      setLoading(true);
      const data = await tagsApi.getAll();
      setTags(data);
      setError(null);
    } catch (err) {
      setError('Nie udało się załadować tagów');
      console.error('Error loading tags:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Czy na pewno chcesz usunąć ten tag?')) {
      return;
    }

    try {
      await tagsApi.delete(id);
      await loadTags();
    } catch (err) {
      setError('Nie udało się usunąć tagu');
      console.error('Error deleting tag:', err);
    }
  };

  const handleEdit = (tag: Tag) => {
    setEditingTag(tag);
    setShowForm(true);
  };

  const handleCreate = () => {
    setEditingTag(null);
    setShowForm(true);
  };

  const handleFormClose = () => {
    setShowForm(false);
    setEditingTag(null);
  };

  const handleFormSuccess = async () => {
    await loadTags();
    handleFormClose();
  };

  if (loading) {
    return (
      <div className="d-flex justify-content-center p-5">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Ładowanie...</span>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Zarządzanie tagami</h2>
        <button className="btn btn-primary" onClick={handleCreate}>
          <i className="bi bi-plus-circle me-2"></i>
          Dodaj tag
        </button>
      </div>

      {error && (
        <div className="alert alert-danger" role="alert">
          {error}
        </div>
      )}

      {tags.length === 0 ? (
        <div className="alert alert-info">
          Brak tagów. Utwórz pierwszy tag aby rozpocząć organizację treści.
        </div>
      ) : (
        <div className="row g-3">
          {tags.map(tag => (
            <div key={tag.id} className="col-md-6 col-lg-4">
              <div className="card h-100">
                <div className="card-body">
                  <div className="d-flex justify-content-between align-items-start mb-2">
                    <div>
                      <h5 className="card-title mb-1">{tag.name}</h5>
                      <small className="text-muted">/{tag.slug}</small>
                    </div>
                    {tag.color && (
                      <div
                        style={{
                          width: '30px',
                          height: '30px',
                          backgroundColor: tag.color,
                          borderRadius: '4px',
                          border: '1px solid #dee2e6'
                        }}
                        title={tag.color}
                      />
                    )}
                  </div>
                  <small className="text-muted d-block mb-3">
                    Utworzono: {new Date(tag.createdAt).toLocaleDateString('pl-PL')}
                  </small>
                  <div className="d-flex gap-2">
                    <button
                      className="btn btn-sm btn-outline-primary"
                      onClick={() => handleEdit(tag)}
                    >
                      <i className="bi bi-pencil me-1"></i>
                      Edytuj
                    </button>
                    <button
                      className="btn btn-sm btn-outline-danger"
                      onClick={() => handleDelete(tag.id)}
                    >
                      <i className="bi bi-trash me-1"></i>
                      Usuń
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {showForm && (
        <TagFormModal
          tag={editingTag}
          onClose={handleFormClose}
          onSuccess={handleFormSuccess}
        />
      )}
    </div>
  );
}

interface TagFormModalProps {
  tag: Tag | null;
  onClose: () => void;
  onSuccess: () => void;
}

function TagFormModal({ tag, onClose, onSuccess }: TagFormModalProps) {
  const [name, setName] = useState(tag?.name || '');
  const [color, setColor] = useState(tag?.color || '#6366f1');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!name.trim()) {
      setError('Nazwa tagu jest wymagana');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const request: CreateTagRequest | UpdateTagRequest = {
        name: name.trim(),
        color: color || undefined
      };

      if (tag) {
        await tagsApi.update(tag.id, request);
      } else {
        await tagsApi.create(request);
      }

      onSuccess();
    } catch (err) {
      setError(tag ? 'Nie udało się zaktualizować tagu' : 'Nie udało się utworzyć tagu');
      console.error('Error saving tag:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <div className="modal show d-block" tabIndex={-1}>
        <div className="modal-dialog modal-dialog-centered">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title">
                {tag ? 'Edytuj tag' : 'Nowy tag'}
              </h5>
              <button
                type="button"
                className="btn-close"
                onClick={onClose}
                disabled={loading}
              ></button>
            </div>
            <form onSubmit={handleSubmit}>
              <div className="modal-body">
                {error && (
                  <div className="alert alert-danger" role="alert">
                    {error}
                  </div>
                )}

                <div className="mb-3">
                  <label htmlFor="tagName" className="form-label">
                    Nazwa tagu *
                  </label>
                  <input
                    type="text"
                    className="form-control"
                    id="tagName"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="np. ASP.NET Core, React, Tutorial"
                    required
                    disabled={loading}
                  />
                  <small className="text-muted">
                    Slug zostanie wygenerowany automatycznie
                  </small>
                </div>

                <div className="mb-3">
                  <label htmlFor="tagColor" className="form-label">
                    Kolor (opcjonalny)
                  </label>
                  <div className="input-group">
                    <input
                      type="color"
                      className="form-control form-control-color"
                      id="tagColor"
                      value={color}
                      onChange={(e) => setColor(e.target.value)}
                      disabled={loading}
                      title="Wybierz kolor"
                    />
                    <input
                      type="text"
                      className="form-control"
                      value={color}
                      onChange={(e) => setColor(e.target.value)}
                      placeholder="#6366f1"
                      disabled={loading}
                    />
                  </div>
                  <small className="text-muted">
                    Kolor pomoże wyróżnić tag w interfejsie
                  </small>
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={onClose}
                  disabled={loading}
                >
                  Anuluj
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={loading}
                >
                  {loading ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2"></span>
                      Zapisywanie...
                    </>
                  ) : (
                    <>{tag ? 'Zapisz zmiany' : 'Utwórz tag'}</>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
      <div className="modal-backdrop show"></div>
    </>
  );
}
