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
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [previewUrls, setPreviewUrls] = useState<string[]>([]);
  const [showCategoryDropdown, setShowCategoryDropdown] = useState(false);

  // Extract unique categories from products
  const categories = useMemo(() => {
    const uniqueCategories = [...new Set(products.map(p => p.category).filter((c): c is string => !!c))];
    return uniqueCategories.sort();
  }, [products]);

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
    // Cleanup preview URLs
    previewUrls.forEach(url => URL.revokeObjectURL(url));
    setSelectedFiles([]);
    setPreviewUrls([]);
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
        // When creating, include selected files
        await create(payload, form.tagIds, selectedFiles.length > 0 ? selectedFiles : undefined);
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
                      <div className="col-md-6 position-relative">
                        <label className="form-label">Kategoria</label>
                        <input
                          type="text"
                          className="form-control"
                          value={form.category}
                          onChange={(e) => {
                            setForm({ ...form, category: e.target.value });
                            setShowCategoryDropdown(true);
                          }}
                          onFocus={() => setShowCategoryDropdown(true)}
                          onBlur={() => setTimeout(() => setShowCategoryDropdown(false), 200)}
                          disabled={isSaving}
                          placeholder="Wpisz lub wybierz kategorię"
                          autoComplete="off"
                        />
                        {showCategoryDropdown && (form.category || categories.length > 0) && (
                          <ul className="list-group position-absolute w-100 mt-1" style={{ zIndex: 1000 }}>
                            {categories
                              .filter((cat) =>
                                cat.toLowerCase().includes(form.category.toLowerCase())
                              )
                              .map((cat) => (
                                <li
                                  key={cat}
                                  className="list-group-item list-group-item-action cursor-pointer"
                                  onClick={() => {
                                    setForm({ ...form, category: cat });
                                    setShowCategoryDropdown(false);
                                  }}
                                  style={{ cursor: 'pointer' }}
                                >
                                  {cat}
                                </li>
                              ))}
                            {form.category &&
                              !categories.includes(form.category) && (
                                <li
                                  className="list-group-item list-group-item-action list-group-item-info"
                                  style={{ cursor: 'pointer' }}
                                >
                                  <i className="bi bi-plus-circle"></i> Nowa kategoria: <strong>{form.category}</strong>
                                </li>
                              )}
                            {categories.filter((cat) =>
                              cat.toLowerCase().includes(form.category.toLowerCase())
                            ).length === 0 &&
                              !form.category && (
                                <li className="list-group-item text-muted text-center" style={{ fontSize: '0.9rem' }}>
                                  Dostępne kategorie
                                </li>
                              )}
                          </ul>
                        )}
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
                        {editingProduct && form.imageUrl && (
                          <div className="mb-2">
                            <img 
                              src={form.imageUrl} 
                              alt="Obecny obraz produktu" 
                              className="img-thumbnail"
                              style={{ maxHeight: '150px', objectFit: 'cover' }}
                              onError={(e) => {
                                e.currentTarget.style.display = 'none';
                              }}
                            />
                          </div>
                        )}
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
                      {editingProduct ? (
                        <ProductThumbnailUpload
                          productId={editingProduct.id}
                          onUploaded={async (imageUrl: string) => {
                            setForm((f) => ({ ...f, imageUrl }));
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
                          }}
                          disabled={isSaving}
                        />
                      ) : (
                        <div>
                          <label className="form-label">Zdjęcia produktu (opcjonalne)</label>
                          <input
                            type="file"
                            className="form-control"
                            accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                            multiple
                            onChange={(e) => {
                              const files = Array.from(e.target.files || []);
                              
                              // Validate file sizes
                              const maxSize = 10 * 1024 * 1024; // 10 MB
                              const invalidFiles = files.filter(f => f.size > maxSize);
                              if (invalidFiles.length > 0) {
                                setMessage({ type: 'error', text: `Niektóre pliki przekraczają limit 10 MB: ${invalidFiles.map(f => f.name).join(', ')}` });
                                return;
                              }

                              // Validate file types
                              const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
                              const invalidTypes = files.filter(f => !allowedTypes.includes(f.type));
                              if (invalidTypes.length > 0) {
                                setMessage({ type: 'error', text: `Niektóre pliki mają nieprawidłowy typ: ${invalidTypes.map(f => f.name).join(', ')}` });
                                return;
                              }

                              setSelectedFiles(files);
                              
                              // Create local previews
                              const previews = files.map(file => URL.createObjectURL(file));
                              setPreviewUrls(previews);
                              setMessage(null);
                            }}
                            disabled={isSaving}
                          />
                          <small className="text-muted">
                            Maksymalnie 10 MB na plik. Formaty: JPEG, PNG, GIF, WebP
                          </small>
                          
                          {previewUrls.length > 0 && (
                            <div className="mt-3">
                              <h6>Podgląd ({previewUrls.length} {previewUrls.length === 1 ? 'zdjęcie' : 'zdjęć'}):</h6>
                              <div className="row g-2">
                                {previewUrls.map((url, index) => (
                                  <div key={index} className="col-4 position-relative">
                                    <img 
                                      src={url} 
                                      alt={`Podgląd ${index + 1}`} 
                                      className="img-thumbnail w-100" 
                                      style={{ height: '150px', objectFit: 'cover' }}
                                    />
                                    <button
                                      type="button"
                                      className="btn btn-sm btn-danger position-absolute top-0 end-0 m-1"
                                      onClick={() => {
                                        URL.revokeObjectURL(url);
                                        setSelectedFiles(prev => prev.filter((_, i) => i !== index));
                                        setPreviewUrls(prev => prev.filter((_, i) => i !== index));
                                      }}
                                      disabled={isSaving}
                                      title="Usuń zdjęcie"
                                    >
                                      <i className="bi bi-x"></i>
                                    </button>
                                  </div>
                                ))}
                              </div>
                            </div>
                          )}
                        </div>
                      )}
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
