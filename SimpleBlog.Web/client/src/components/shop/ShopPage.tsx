import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useProducts } from '@/hooks/useProducts';
import { useCart } from '@/hooks/useCart';
import { ProductSearchBar } from './ProductSearchBar';
import { TagBadges } from '@/components/common/TagSelector';
import { getColorName } from '@/utils/colorNames';

interface ShopPageProps {
  onViewCart?: () => void;
}

export function ShopPage({ onViewCart }: ShopPageProps) {
  const { products, loading, error, setFilter } = useProducts();
  const { addItem, itemCount } = useCart();
  const navigate = useNavigate();
  const [addedProduct, setAddedProduct] = useState<string | null>(null);
  const [selectedProduct, setSelectedProduct] = useState<any | null>(null);

  // Extract unique categories
  const categories = useMemo(() => {
    const uniqueCategories = [...new Set(products.map(p => p.category).filter((c): c is string => !!c))];
    return uniqueCategories.sort();
  }, [products]);

  const handleAddToCart = (e: React.MouseEvent, product: any) => {
    e.stopPropagation();
    const defaultColor = product.colors && product.colors.length > 0 ? product.colors[0] : undefined;
    addItem(product, 1, defaultColor);
    setAddedProduct(product.id);

    // Show notification for 2 seconds
    setTimeout(() => setAddedProduct(null), 2000);
  };

  if (loading) return <p className="text-muted">Ładowanie...</p>;
  if (error) return <div className="alert alert-danger">{error}</div>;

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2 className="mb-0">Sklep</h2>
        {itemCount > 0 && (
          <button
            className="btn btn-primary position-relative"
            onClick={onViewCart}
          >
            <i className="bi bi-cart3 me-2"></i>
            Pokaż koszyk
            <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
              {itemCount}
            </span>
          </button>
        )}
      </div>

      <ProductSearchBar onSearch={setFilter} categories={categories} />

      {products.length === 0 ? (
        <div className="alert alert-info">
          <i className="bi bi-info-circle me-2"></i>
          Brak produktów spełniających kryteria.
        </div>
      ) : (
      <div className="row g-4">
        {products.map((product) => (
          <div key={product.id} className="col-12 col-md-6 col-lg-6">
            <div className="card shadow-sm h-100 position-relative product-card" style={{ cursor: 'pointer' }} onClick={() => setSelectedProduct(product)}>
              {product.imageUrl && (
                <img
                  src={product.imageUrl}
                  className="card-img-top"
                  style={{ height: '250px', objectFit: 'cover' }}
                  alt={product.name}
                />
              )}
              <div className="card-body d-flex flex-column">
                {addedProduct === product.id && (
                  <div role="status" aria-live="polite" className="alert alert-success alert-sm py-1 px-2 mb-2 w-100 text-center" style={{ fontSize: '0.9rem' }}>
                    <i className="bi bi-check-circle me-1"></i>Dodano do koszyka!
                  </div>
                )}
                <h5 className="card-title">{product.name}</h5>
                {product.colors && product.colors.length > 0 && (
                  <div className="d-flex align-items-center mb-2">
                    {product.colors[0].startsWith('http') ? (
                      <div className="color-swatch me-2" style={{ width: 20, height: 20 }}><img src={product.colors[0]} alt="default-color" style={{ width: 20, height: 20, objectFit: 'cover' }} /></div>
                    ) : (
                      <div className="color-swatch me-2" style={{ width: 20, height: 20, backgroundColor: product.colors[0] }} />
                    )}
                    <small className="text-muted">{getColorName(product.colors[0])}</small>
                  </div>
                )}
                {product.tags && product.tags.length > 0 && (
                  <div className="mb-2">
                    <TagBadges tags={product.tags} />
                  </div>
                )}
                <p className="card-text text-muted flex-grow-1">
                  {product.description.substring(0, 80)}...
                </p>
                <div className="d-flex justify-content-between align-items-center mt-3">
                  <span className="h5 mb-0 text-primary">{product.price.toFixed(2)} PLN</span>
                  {product.stock > 0 ? (
                    <button 
                      className="btn btn-outline-primary btn-sm"
                      onClick={(e) => handleAddToCart(e, product)}
                    >
                      <i className="bi bi-cart-plus me-1"></i>Do koszyka
                    </button>
                  ) : (
                    <span className="badge bg-danger">Brak w magazynie</span>
                  )}
                </div>
                <small className="text-muted mt-2">
                  <i className="bi bi-box me-1"></i>Dostępne: {product.stock}
                </small>
              </div>
            </div>
          </div>
        ))}
      </div>
      )}
      {selectedProduct && (
        <>
          <div className="modal-backdrop show" onClick={() => setSelectedProduct(null)}></div>
          <div className="modal show d-block" tabIndex={-1} style={{ overflowY: 'auto' }}>
            <div className="modal-dialog modal-lg modal-dialog-scrollable">
              <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                <div className="modal-header">
                  <div>
                    <h3 className="modal-title">{selectedProduct.name}</h3>
                    <div className="mt-2">
                      <small className="text-muted me-3">
                        <i className="bi bi-tag me-1"></i>
                        {selectedProduct.category}
                      </small>
                    </div>
                  </div>
                  <div className="d-flex gap-2">
                    <button
                      type="button"
                      className="btn btn-sm btn-outline-primary"
                      onClick={() => {
                        const url = `${window.location.origin}/shop/${selectedProduct.id}`;
                        navigator.clipboard.writeText(url);
                        alert('Link skopiowany do schowka!');
                      }}
                      title="Udostępnij link"
                    >
                      <i className="bi bi-share"></i>
                    </button>
                    <button
                      type="button"
                      className="btn btn-sm btn-outline-info"
                      onClick={() => {
                        setSelectedProduct(null);
                        navigate(`/shop/${selectedProduct.id}`);
                      }}
                      title="Otwórz pełny widok"
                    >
                      <i className="bi bi-box-arrow-up-right"></i>
                    </button>
                    <button
                      type="button"
                      className="btn-close"
                      onClick={() => setSelectedProduct(null)}
                      aria-label="Close"
                    ></button>
                  </div>
                </div>
                <div className="modal-body">
                  <div className="row g-0">
                    {selectedProduct.imageUrl && (
                      <div className="col-md-5 col-lg-4 mb-4 mb-md-0">
                        <div className="sticky-top" style={{ top: '20px' }}>
                          <div className="position-relative rounded overflow-hidden mb-3" style={{ height: '300px', width: '100%' }}>
                            <img src={selectedProduct.imageUrl} className="position-relative img-fluid" style={{ height: '300px', width: '100%', objectFit: 'cover', zIndex: 1 }} alt={selectedProduct.name} />
                          </div>
                        </div>
                      </div>
                    )}
                    <div className={selectedProduct.imageUrl ? 'col-md-7 col-lg-8' : 'col-12'}>
                      <div className={selectedProduct.imageUrl ? 'ps-md-4' : ''}>
                        {selectedProduct.tags && selectedProduct.tags.length > 0 && (
                          <div className="mb-3">
                            <TagBadges tags={selectedProduct.tags} />
                          </div>
                        )}
                            {selectedProduct.colors && selectedProduct.colors.length > 0 && (
                          <div className="mb-3">
                            <div className="color-swatches">
                              {selectedProduct.colors.map((c: string, i: number) => (
                                <div key={i} className="d-flex align-items-center me-3" style={{ cursor: 'pointer' }} onClick={(e) => { e.stopPropagation(); /* local selection not persisted for modal quick-add */ }}>
                                  {c.startsWith('http') ? (
                                    <div className="color-swatch" title={c}><img className="color-swatch-img" src={c} alt={`color-${i}`} /></div>
                                  ) : (
                                    <div className="color-swatch" title={c} style={{ backgroundColor: c }} />
                                  )}
                                  <div className="color-swatch-label">{getColorName(c)}</div>
                                </div>
                              ))}
                            </div>
                          </div>
                        )}
                        <p className="fs-5 lh-lg" style={{ whiteSpace: 'pre-wrap' }}>
                          {selectedProduct.description}
                        </p>
                        <div className="d-flex justify-content-between align-items-center mt-3">
                          <span className="h5 mb-0 text-primary">{selectedProduct.price.toFixed(2)} PLN</span>
                            {selectedProduct.stock > 0 ? (
                            <button
                              className="btn btn-outline-primary btn-sm"
                              onClick={(e) => {
                                e.stopPropagation();
                                const defaultColor = selectedProduct.colors && selectedProduct.colors.length > 0 ? selectedProduct.colors[0] : undefined;
                                addItem(selectedProduct, 1, defaultColor);
                                setSelectedProduct(null);
                              }}
                            >
                              <i className="bi bi-cart-plus me-1"></i>Do koszyka
                            </button>
                          ) : (
                            <span className="badge bg-danger">Brak w magazynie</span>
                          )}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </>
      )}
    </>
  );
}
