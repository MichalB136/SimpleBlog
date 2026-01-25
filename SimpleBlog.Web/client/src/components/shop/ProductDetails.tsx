import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { productsApi } from '@/api/products';
import type { Product } from '@/types/product';
import { TagBadges } from '@/components/common/TagSelector';
import { getColorName } from '@/utils/colorNames';
import { useCart } from '@/hooks/useCart';

export function ProductDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { addItem } = useCart();
  const [product, setProduct] = useState<Product | null>(null);
  const [selectedColor, setSelectedColor] = useState<string | undefined>(undefined);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    productsApi.getById(id)
      .then((res) => setProduct(res))
      .catch((err) => setError(err?.message ?? 'Nie można pobrać produktu'))
      .finally(() => setLoading(false));
  }, [id]);

  // Select default color when product is loaded
  useEffect(() => {
    if (product && product.colors && product.colors.length > 0) {
      setSelectedColor((prev) => prev ?? product.colors![0]);
    }
  }, [product]);

  if (loading) return <p className="text-muted">Ładowanie...</p>;
  if (error) return <div className="alert alert-danger">{error}</div>;
  if (!product) return <div className="alert alert-info">Produkt nie znaleziony.</div>;

  return (
    <div className="container my-4">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <button className="btn btn-link" onClick={() => navigate(-1)}>
          <i className="bi bi-arrow-left me-2"></i>Powrót
        </button>
        <h2 className="mb-0">{product.name}</h2>
        <div />
      </div>

      <div className="row g-4">
        <div className="col-md-5 col-lg-4">
          {product.imageUrl ? (
            <div className="position-relative rounded overflow-hidden mb-3" style={{ height: '400px' }}>
              <img src={product.imageUrl} alt={product.name} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
            </div>
          ) : (
            <div className="bg-light d-flex align-items-center justify-content-center rounded" style={{ height: '400px' }}>
              <i className="bi bi-image text-muted" style={{ fontSize: '3rem' }} />
            </div>
          )}

                    {product.colors && product.colors.length > 0 && (
            <div className="mt-3">
              <h6>Kolory / Wzory</h6>
              <div className="color-swatches mt-2">
                {product.colors.map((c, i) => (
                  <div key={i} className="d-flex align-items-center me-3" style={{ cursor: 'pointer' }} onClick={() => setSelectedColor(c)}>
                    {c.startsWith('http') ? (
                      <div className={`color-swatch ${selectedColor === c ? 'border border-3 border-primary' : ''}`} title={c}><img className="color-swatch-img" src={c} alt={`color-${i}`} /></div>
                    ) : (
                      <div className={`color-swatch ${selectedColor === c ? 'border border-3 border-primary' : ''}`} title={c} style={{ backgroundColor: c }} />
                    )}
                    <div className="color-swatch-label">{getColorName(c)}</div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        <div className="col-md-7 col-lg-8">
          <div className="card p-4">
            <div className="mb-3">
              {product.tags && product.tags.length > 0 && <TagBadges tags={product.tags} />}
            </div>
            <p className="fs-5" style={{ whiteSpace: 'pre-wrap' }}>{product.description}</p>
            <div className="d-flex justify-content-between align-items-center mt-4">
              <div>
                <div className="h4 text-primary mb-1">{product.price.toFixed(2)} PLN</div>
                  <small className="text-muted">Dostępne: {product.stock}</small>
                  {selectedColor && (
                    <div className="mt-2 small text-muted">Wybrany kolor: {getColorName(selectedColor)}</div>
                  )}
              </div>
              <div>
                {product.stock > 0 ? (
                  <button className="btn btn-primary" onClick={() => { addItem(product, 1, selectedColor); navigate('/cart'); }}>
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
  );
}

export default ProductDetails;
