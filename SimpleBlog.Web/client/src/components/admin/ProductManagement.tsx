import { useEffect, useMemo, useState } from 'react';
import { useProducts } from '@/hooks/useProducts';
import { TagSelector } from '@/components/common/TagSelector';
import { ColorSelector } from '@/components/common/ColorSelector';
import ProductThumbnailUpload from '@/components/admin/ProductThumbnailUpload';
import type { Product } from '@/types/product';

interface ProductFormState {
  name: string;
  description: string;
  price: string;
  stock: string;
  category: string;
  imageUrl: string;
  tagIds: string[];
  colors: string[];
}

const emptyForm: ProductFormState = {
  name: '',
  description: '',
  price: '',
  stock: '',
  category: '',
  imageUrl: '',
  tagIds: [],
  colors: [],
};

export function ProductManagement() {
  const { products, loading, error, create, update, delete: deleteProduct, refresh, setError } = useProducts();
  const [isSaving, setIsSaving] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [form, setForm] = useState<ProductFormState>(emptyForm);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  useEffect(() => {
    if (editingProduct) {
      setForm({
        name: editingProduct.name,
        description: editingProduct.description,
        price: editingProduct.price.toString(),
        stock: editingProduct.stock.toString(),
        category: editingProduct.category ?? '',
        imageUrl: editingProduct.imageUrl ?? '',
        tagIds: editingProduct.tags?.map((t) => t.id) ?? [],
        colors: editingProduct.colors ?? [],
      });
    } else {
      setForm(emptyForm);
    }
  }, [editingProduct]);

  

  const sortedProducts = useMemo(
    () => [...products].sort((a, b) => new Date(b.createdAt ?? '').getTime() - new Date(a.createdAt ?? '').getTime()),
    [products]
  );

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingProduct(null);
    setForm(emptyForm);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);

    if (!form.name.trim() || !form.description.trim()) {
      setMessage({ type: 'error', text: 'Nazwa i opis są wymagane' });
      return;
    }

    const payload = {
      name: form.name.trim(),
      description: form.description.trim(),
      price: Number(form.price) || 0,
      stock: Number(form.stock) || 0,
      category: form.category.trim() || undefined,
      imageUrl: form.imageUrl.trim() || undefined,
      colors: form.colors && form.colors.length > 0 ? form.colors : undefined,
    };

    setIsSaving(true);
    try {
      if (editingProduct) {
        await update(editingProduct.id, payload, form.tagIds);
        setMessage({ type: 'success', text: 'Produkt zaktualizowany' });
      } else {
        await create(payload, form.tagIds);
        setMessage({ type: 'success', text: 'Produkt utworzony' });
      }
      handleCloseModal();
      await refresh();
    } catch (err) {
      setMessage({ type: 'error', text: err instanceof Error ? err.message : 'Błąd zapisu produktu' });
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!globalThis.confirm('Usunąć produkt?')) return;
    try {
      await deleteProduct(id);
      setMessage({ type: 'success', text: 'Produkt usunięty' });
    } catch (err) {
      setMessage({ type: 'error', text: err instanceof Error ? err.message : 'Błąd usuwania produktu' });
    }
  };

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h5 className="mb-0">Zarządzanie produktami</h5>
        <button
          className="btn btn-primary"
          onClick={() => {
            setEditingProduct(null);
            setForm(emptyForm);
            setShowModal(true);
          }}
        >
          <i className="bi bi-plus-circle me-2"></i>Dodaj produkt
        </button>
      </div>

      {message && (
        <div className={`alert alert-${message.type === 'success' ? 'success' : 'danger'} alert-dismissible fade show`}>
          {message.text}
          <button type="button" className="btn-close" onClick={() => setMessage(null)} aria-label="Close"></button>
        </div>
      )}

      {error && (
        <div className="alert alert-danger" role="alert">
          {error}
          <button className="btn btn-sm btn-outline-light ms-2" onClick={() => setError('')}>
            Wyczyść
          </button>
        </div>
      )}

      {loading ? (
        <div className="text-center py-4" aria-live="polite">
          <div className="spinner-border" role="status" aria-hidden="true"></div>
        </div>
      ) : sortedProducts.length === 0 ? (
        <div className="alert alert-info">Brak produktów. Dodaj pierwszy produkt.</div>
      ) : (
        <div className="table-responsive">
          <table className="table table-hover align-middle">
            <thead>
              <tr>
                <th>Nazwa</th>
                <th>Kategoria</th>
                <th>Cena</th>
                <th>Stan</th>
                <th>Tagi</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {sortedProducts.map((product) => (
                <tr key={product.id}>
                  <td>
                    <strong>{product.name}</strong>
                    <div className="text-muted small">{product.description}</div>
                  </td>
                  <td>{product.category || '—'}</td>
                  <td>{product.price.toFixed(2)} PLN</td>
                  <td>{product.stock}</td>
                  <td>
                    {product.tags && product.tags.length > 0 ? (
                      <div className="d-flex flex-wrap gap-1">
                        {product.tags.map((tag) => (
                          <span key={tag.id} className="badge" style={{ backgroundColor: tag.color || '#6c757d' }}>
                            {tag.name}
                          </span>
                        ))}
                      </div>
                    ) : (
                      <span className="text-muted">Brak</span>
                    )}
                    {product.colors && product.colors.length > 0 && (
                      <div className="mt-2 d-flex gap-1">
                        {product.colors.map((c) => (
                          <span key={c} title={c} className="rounded-circle" style={{ display: 'inline-block', width: 16, height: 16, background: c, border: '1px solid #ccc' }}></span>
                        ))}
                      </div>
                    )}
                  </td>
                  <td className="text-end">
                    <div className="btn-group btn-group-sm">
                      <button
                        className="btn btn-outline-primary"
                        onClick={() => {
                          setEditingProduct(product);
                          setShowModal(true);
                        }}
                      >
                        <i className="bi bi-pencil"></i>
                      </button>
                      <button className="btn btn-outline-danger" onClick={() => handleDelete(product.id)}>
                        <i className="bi bi-trash"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {showModal && (
        <>
          <div className="modal-backdrop show" onClick={handleCloseModal}></div>
          <div className="modal show d-block" tabIndex={-1} role="dialog" aria-modal="true">
            <div className="modal-dialog modal-lg">
              <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                <div className="modal-header">
                  <h5 className="modal-title">{editingProduct ? 'Edytuj produkt' : 'Dodaj produkt'}</h5>
                  <button type="button" className="btn-close" aria-label="Close" onClick={handleCloseModal}></button>
                </div>
                <form onSubmit={handleSubmit}>
                  <div className="modal-body">
                    <div className="row g-3">
                      <div className="col-md-6">
                        <label className="form-label">Nazwa *</label>
                        <input
                          type="text"
                          className="form-control"
                          value={form.name}
                          onChange={(e) => setForm({ ...form, name: e.target.value })}
                          required
                          disabled={isSaving}
                        />
                      </div>
                      <div className="col-md-6">
                        <label className="form-label">Kategoria</label>
                        <input
                          type="text"
                          className="form-control"
                          value={form.category}
                          onChange={(e) => setForm({ ...form, category: e.target.value })}
                          disabled={isSaving}
                          placeholder="np. Sukienki, Akcesoria, Koszulki"
                        />
                      </div>
                      <div className="col-12">
                        <label className="form-label">Opis *</label>
                        <textarea
                          className="form-control"
                          rows={3}
                          value={form.description}
                          onChange={(e) => setForm({ ...form, description: e.target.value })}
                          required
                          disabled={isSaving}
                        ></textarea>
                      </div>
                      <div className="col-md-4">
                        <label className="form-label">Cena (PLN) *</label>
                        <input
                          type="number"
                          step="0.01"
                          min="0"
                          className="form-control"
                          value={form.price}
                          onChange={(e) => setForm({ ...form, price: e.target.value })}
                          required
                          disabled={isSaving}
                        />
                      </div>
                      <div className="col-md-4">
                        <label className="form-label">Stan magazynowy *</label>
                        <input
                          type="number"
                          min="0"
                          className="form-control"
                          value={form.stock}
                          onChange={(e) => setForm({ ...form, stock: e.target.value })}
                          required
                          disabled={isSaving}
                        />
                      </div>
                      <div className="col-md-4">
                        <label className="form-label">URL zdjęcia</label>
                        <input
                          type="url"
                          className="form-control"
                          value={form.imageUrl}
                          onChange={(e) => setForm({ ...form, imageUrl: e.target.value })}
                          disabled={isSaving}
                          placeholder="https://..."
                        />
                      </div>
                    </div>

                    <div className="col-12 mt-2">
                      <ProductThumbnailUpload
                        productId={editingProduct ? editingProduct.id : null}
                        onUploaded={async (imageUrl: string) => {
                          setForm((f) => ({ ...f, imageUrl }));
                          if (editingProduct) {
                            setIsSaving(true);
                            try {
                              const payload = {
                                name: form.name.trim(),
                                description: form.description.trim(),
                                price: Number(form.price) || 0,
                                stock: Number(form.stock) || 0,
                                category: form.category.trim() || undefined,
                                imageUrl: imageUrl || undefined,
                                colors: form.colors && form.colors.length > 0 ? form.colors : undefined,
                              };
                              await update(editingProduct.id, payload, form.tagIds);
                              await refresh();
                              setMessage({ type: 'success', text: 'Zdjęcie przesłane i zapisane' });
                            } catch (err: any) {
                              setMessage({ type: 'error', text: err?.message || 'Błąd zapisu zdjęcia' });
                            } finally {
                              setIsSaving(false);
                            }
                          }
                        }}
                        disabled={isSaving}
                      />
                    </div>

                    <div className="mt-3">
                      <label className="form-label">Tagi (style, materiały, okazje)</label>
                      <TagSelector
                        selectedTagIds={form.tagIds}
                        onChange={(ids) => setForm({ ...form, tagIds: ids })}
                        disabled={isSaving}
                      />
                      <small className="text-muted">Wybierz tagi, aby filtrować ręcznie robione ubrania.</small>
                    </div>
                    <div className="mt-3">
                      <label className="form-label">Dostępne kolory</label>
                      <ColorSelector colors={form.colors} onChange={(c) => setForm({ ...form, colors: c })} disabled={isSaving} />
                    </div>
                  </div>
                  <div className="modal-footer">
                    <button type="button" className="btn btn-secondary" onClick={handleCloseModal} disabled={isSaving}>
                      Anuluj
                    </button>
                    <button type="submit" className="btn btn-primary" disabled={isSaving}>
                      {isSaving ? (
                        <>
                          <span className="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>
                          Zapisywanie...
                        </>
                      ) : (
                        <>
                          <i className="bi bi-save me-2"></i>
                          Zapisz
                        </>
                      )}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
