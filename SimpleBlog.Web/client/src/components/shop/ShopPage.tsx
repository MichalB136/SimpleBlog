import { useProducts } from '@/hooks/useProducts';

export function ShopPage() {
  const { products, loading, error } = useProducts();
  // Admin-only features can be added later

  if (loading) return <p className="text-muted">Ładowanie...</p>;
  if (error) return <div className="alert alert-danger">{error}</div>;

  if (products.length === 0) {
    return (
      <div className="alert alert-info">
        <i className="bi bi-info-circle me-2"></i>
        Brak produktów w sklepie.
      </div>
    );
  }

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2 className="mb-0">Sklep</h2>
      </div>
      <div className="row g-4">
        {products.map((product) => (
          <div key={product.id} className="col-md-6 col-lg-4">
            <div className="card shadow-sm h-100">
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
                <p className="card-text text-muted flex-grow-1">
                  {product.description.substring(0, 80)}...
                </p>
                <div className="d-flex justify-content-between align-items-center mt-3">
                  <span className="h5 mb-0 text-primary">{product.price.toFixed(2)} PLN</span>
                  {product.stock > 0 ? (
                    <button className="btn btn-outline-primary btn-sm">
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
    </>
  );
}
