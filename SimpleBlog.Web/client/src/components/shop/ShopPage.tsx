import { useState, useMemo } from 'react';
import { useProducts } from '@/hooks/useProducts';
import { useCart } from '@/hooks/useCart';
import { ProductSearchBar } from './ProductSearchBar';
import { TagBadges } from '@/components/common/TagSelector';

interface ShopPageProps {
  onViewCart?: () => void;
}

export function ShopPage({ onViewCart }: ShopPageProps) {
  const { products, loading, error, setFilter } = useProducts();
  const { addItem, itemCount } = useCart();
  const [addedProduct, setAddedProduct] = useState<string | null>(null);

  // Extract unique categories
  const categories = useMemo(() => {
    const uniqueCategories = [...new Set(products.map(p => p.category).filter((c): c is string => !!c))];
    return uniqueCategories.sort();
  }, [products]);

  const handleAddToCart = (product: any) => {
    addItem(product, 1);
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
          <div key={product.id} className="col-md-6 col-lg-4">
            <div className="card shadow-sm h-100 position-relative">
              {addedProduct === product.id && (
                <div className="position-absolute top-0 start-50 translate-middle-x mt-2 alert alert-success alert-sm py-1 px-2" style={{ fontSize: '0.85rem' }}>
                  <i className="bi bi-check-circle me-1"></i>Dodano do koszyka!
                </div>
              )}
              {product.imageUrl && (
                <img
                  src={product.imageUrl}
                  className="card-img-top"
                  style={{ height: '250px', objectFit: 'cover' }}
                  alt={product.name}
                />
              )}
              <div className="card-body d-flex flex-column">
                <h5 className="card-title">{product.name}</h5>
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
                      onClick={() => handleAddToCart(product)}
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
    </>
  );
}
