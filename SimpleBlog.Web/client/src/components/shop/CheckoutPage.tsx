import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCart } from '@/hooks/useCart';
import { getColorName } from '@/utils/colorNames';

export function CheckoutPage() {
  const { items, totalPrice, clearCart } = useCart();
  const navigate = useNavigate();

  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [address, setAddress] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errors, setErrors] = useState<string[]>([]);

  const validate = () => {
    const e: string[] = [];
    if (!name.trim()) e.push('Imię i nazwisko jest wymagane.');
    if (!email.trim() && !phone.trim()) e.push('Podaj e-mail lub numer telefonu.');
    if (!address.trim()) e.push('Adres dostawy jest wymagany.');
    if (items.length === 0) e.push('Koszyk jest pusty.');
    setErrors(e);
    return e.length === 0;
  };

  const handleSubmit = (e?: React.FormEvent) => {
    e?.preventDefault();
    if (!validate()) return;
    setSubmitting(true);

    // Create a simple order record in localStorage (dev placeholder)
    try {
      const ordersJson = localStorage.getItem('orders') || '[]';
      const orders = JSON.parse(ordersJson);
      const order = {
        id: `ord_${Date.now()}`,
        items,
        total: totalPrice,
        contact: { name, email, phone, address },
        status: 'pending',
        createdAt: new Date().toISOString(),
      };
      orders.push(order);
      localStorage.setItem('orders', JSON.stringify(orders));

      clearCart();
      setSuccessMessage('Zamówienie zostało złożone. Twoje zamówienie wymaga akceptacji sklepu.');
      // user stays on confirmation; navigation is manual via button
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="container">
      <h2 className="mb-4">Złóż zamówienie</h2>

      {errors.length > 0 && (
        <div className="alert alert-danger">
          <ul className="mb-0">
            {errors.map((err, i) => (
              <li key={i}>{err}</li>
            ))}
          </ul>
        </div>
      )}

      {successMessage ? (
        <div className="alert alert-success">
          <h5 className="mb-1">Dziękujemy!</h5>
          <p className="mb-0">{successMessage}</p>
          <p className="small text-muted mb-0">Płatność realizowana jest przez zewnętrzny serwis.</p>
          <div className="mt-3 text-center">
            <button className="btn btn-primary" onClick={() => navigate('/')}>Powrót do strony głównej</button>
          </div>
        </div>
      ) : (
      <form onSubmit={handleSubmit}>
        <div className="row">
          <div className="col-md-6">
            <div className="mb-3">
              <label className="form-label">Imię i nazwisko</label>
              <input className="form-control" value={name} onChange={ev => setName(ev.target.value)} />
            </div>

            <div className="mb-3">
              <label className="form-label">E-mail</label>
              <input type="email" className="form-control" value={email} onChange={ev => setEmail(ev.target.value)} />
            </div>

            <div className="mb-3">
              <label className="form-label">Telefon</label>
              <input className="form-control" value={phone} onChange={ev => setPhone(ev.target.value)} />
            </div>

            <div className="mb-3">
              <label className="form-label">Adres dostawy</label>
              <textarea className="form-control" value={address} onChange={ev => setAddress(ev.target.value)} />
            </div>

            <div className="d-flex gap-2">
              <button className="btn btn-secondary" type="button" onClick={() => navigate('/cart')}>Powrót do koszyka</button>
              <button className="btn btn-primary" type="submit" disabled={submitting}>
                {submitting ? 'Wysyłanie...' : 'Złóż zamówienie'}
              </button>
            </div>
          </div>

          <div className="col-md-6">
            <div className="card">
              <div className="card-body">
                <h5 className="card-title">Podsumowanie zamówienia</h5>
                <ul className="list-group list-group-flush mb-3">
                  {items.map(item => (
                    <li key={item.id} className="list-group-item d-flex justify-content-between align-items-center">
                      <div>
                        <strong>{item.name}</strong>
                        <div className="small text-muted">Ilość: {item.quantity}</div>
                        {item.selectedColor && (
                          <div className="small text-muted">Kolor: {getColorName(item.selectedColor)}</div>
                        )}
                      </div>
                      <div>{(item.price * item.quantity).toFixed(2)} zł</div>
                    </li>
                  ))}
                </ul>
                <div className="d-flex justify-content-between">
                  <span>Razem</span>
                  <strong>{totalPrice.toFixed(2)} zł</strong>
                </div>
                <p className="small text-muted mt-3">Po złożeniu zamówienia sklep musi je zaakceptować. Płatność zostanie wykonana poza stroną.</p>
              </div>
            </div>
          </div>
        </div>
      </form>
      )}
    </div>
  );
}

export default CheckoutPage;
