import { useCart } from '@/hooks/useCart';

export function CartPage() {
  const { items, totalPrice, removeItem, updateQuantity, clearCart } = useCart();

  if (items.length === 0) {
    return (
      <div className="alert alert-info text-center">
        <i className="bi bi-cart-x me-2 fs-3"></i>
        <h5>Koszyk jest pusty</h5>
        <p className="text-muted">Dodaj produkty, aby je zobaczyć tutaj</p>
      </div>
    );
  }

  return (
    <div className="card">
      <div className="card-header bg-primary text-white">
        <h5 className="mb-0">
          <i className="bi bi-cart3 me-2"></i>Mój koszyk ({items.length})
        </h5>
      </div>

      <div className="card-body">
        <div className="table-responsive">
          <table className="table table-hover">
            <thead>
              <tr>
                <th>Produkt</th>
                <th>Cena</th>
                <th>Ilość</th>
                <th>Razem</th>
                <th>Akcje</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td>
                    <strong>{item.name}</strong>
                    {item.description && (
                      <p className="text-muted small mb-0">{item.description}</p>
                    )}
                  </td>
                  <td>{item.price.toFixed(2)} zł</td>
                  <td>
                    <div className="input-group input-group-sm" style={{ maxWidth: '120px' }}>
                      <button
                        className="btn btn-outline-secondary"
                        onClick={() => updateQuantity(item.id, item.quantity - 1)}
                      >
                        −
                      </button>
                      <input
                        type="text"
                        className="form-control text-center"
                        value={item.quantity}
                        readOnly
                      />
                      <button
                        className="btn btn-outline-secondary"
                        onClick={() => updateQuantity(item.id, item.quantity + 1)}
                      >
                        +
                      </button>
                    </div>
                  </td>
                  <td>
                    <strong>{(item.price * item.quantity).toFixed(2)} zł</strong>
                  </td>
                  <td>
                    <button
                      className="btn btn-sm btn-danger"
                      onClick={() => removeItem(item.id)}
                      title="Usuń z koszyka"
                    >
                      <i className="bi bi-trash me-1"></i>Usuń
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="row mt-4">
          <div className="col-md-6">
            <button
              className="btn btn-outline-danger w-100"
              onClick={clearCart}
            >
              <i className="bi bi-x-circle me-2"></i>Wyczyść koszyk
            </button>
          </div>
          <div className="col-md-6">
            <div className="card bg-light">
              <div className="card-body">
                <div className="d-flex justify-content-between mb-2">
                  <span>Subtotal:</span>
                  <span>{totalPrice.toFixed(2)} zł</span>
                </div>
                <div className="d-flex justify-content-between mb-2">
                  <span>Podatek (23%):</span>
                  <span>{(totalPrice * 0.23).toFixed(2)} zł</span>
                </div>
                <hr />
                <div className="d-flex justify-content-between mb-3">
                  <strong>Razem:</strong>
                  <strong className="fs-5 text-primary">
                    {(totalPrice * 1.23).toFixed(2)} zł
                  </strong>
                </div>
                <button
                  className="btn btn-primary w-100"
                  disabled
                  title="Funkcja dostępna wkrótce"
                >
                  <i className="bi bi-credit-card me-2"></i>Przejdź do płatności
                </button>
                <p className="text-muted small text-center mt-2 mb-0">
                  Płatność będzie dostępna wkrótce
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
