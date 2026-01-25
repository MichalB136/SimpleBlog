import { useEffect, useState } from 'react';

type TopProduct = {
  productId: string;
  productName: string;
  count: number;
};

type OrderSummary = {
  totalOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
};

type SalesByDay = {
  date: string;
  ordersCount: number;
  revenue: number;
};

export function Analytics() {
  const [topSold, setTopSold] = useState<TopProduct[]>([]);
  const [topViewed, setTopViewed] = useState<TopProduct[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [orderSummary, setOrderSummary] = useState<OrderSummary | null>(null);
  const [salesByDay, setSalesByDay] = useState<SalesByDay[]>([]);

  useEffect(() => {
    async function load() {
      setLoading(true);
      setError(null);
      try {
        const token = localStorage.getItem('authToken');
        const headers: any = { 'Content-Type': 'application/json' };
        if (token) headers.Authorization = `Bearer ${token}`;

        const [soldRes, viewedRes, summaryRes, salesRes] = await Promise.all([
          fetch('/api/products/analytics/top-sold?limit=10', { headers }),
          fetch('/api/products/analytics/top-viewed?limit=10', { headers }),
          fetch('/api/orders/analytics/summary', { headers }),
          fetch('/api/orders/analytics/sales-by-day?limit=30', { headers })
        ]);

        if (!soldRes.ok || !viewedRes.ok || !summaryRes.ok || !salesRes.ok) {
          if (soldRes.status === 401 || viewedRes.status === 401 || summaryRes.status === 401 || salesRes.status === 401) {
            setError('Brak autoryzacji: zaloguj się jako administrator');
          } else {
            setError('Błąd podczas pobierania danych analitycznych');
          }
          setLoading(false);
          return;
        }

        const soldJson = await soldRes.json();
        const viewedJson = await viewedRes.json();
        const summaryJson = await summaryRes.json();
        const salesJson = await salesRes.json();

        setTopSold(soldJson as TopProduct[]);
        setTopViewed(viewedJson as TopProduct[]);
        setOrderSummary(summaryJson as OrderSummary);
        setSalesByDay(salesJson as SalesByDay[]);
      } catch (ex) {
        setError('Błąd sieci podczas pobierania danych');
      } finally {
        setLoading(false);
      }
    }

    load();
  }, []);

  if (loading) return <div className="text-center py-4">Ładowanie danych...</div>;
  if (error) return <div className="alert alert-danger">{error}</div>;

  return (
    <div>
      <h4 className="mb-3">Podsumowanie zamówień</h4>
      {orderSummary ? (
        <div className="row mb-4">
          <div className="col-md-4">
            <div className="card p-3">
              <div className="h6">Liczba zamówień</div>
              <div className="fs-4">{orderSummary.totalOrders}</div>
            </div>
          </div>
          <div className="col-md-4">
            <div className="card p-3">
              <div className="h6">Przychód</div>
              <div className="fs-4">{orderSummary.totalRevenue.toFixed(2)} zł</div>
            </div>
          </div>
          <div className="col-md-4">
            <div className="card p-3">
              <div className="h6">Średnia wartość zamówienia</div>
              <div className="fs-4">{orderSummary.averageOrderValue.toFixed(2)} zł</div>
            </div>
          </div>
        </div>
      ) : null}

      <h4 className="mb-3">Sprzedaż — ostatnie dni</h4>
      <div className="table-responsive mb-4">
        <table className="table table-striped">
          <thead>
            <tr>
              <th>Data</th>
              <th>Zamówień</th>
              <th>Przychód</th>
            </tr>
          </thead>
          <tbody>
            {salesByDay.map(s => (
              <tr key={s.date}>
                <td>{new Date(s.date).toLocaleDateString('pl-PL')}</td>
                <td>{s.ordersCount}</td>
                <td>{s.revenue.toFixed(2)} zł</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <h4 className="mb-3">Najczęściej zamawiane produkty</h4>
      <div className="table-responsive mb-4">
        <table className="table table-striped">
          <thead>
            <tr>
              <th>#</th>
              <th>Produkt</th>
              <th>Ilość</th>
            </tr>
          </thead>
          <tbody>
            {topSold.map((p, i) => (
              <tr key={p.productId}>
                <td>{i + 1}</td>
                <td>{p.productName}</td>
                <td>{p.count}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <h4 className="mb-3">Najczęściej przeglądane</h4>
      <div className="table-responsive">
        <table className="table table-striped">
          <thead>
            <tr>
              <th>#</th>
              <th>Produkt</th>
              <th>Wyświetleń</th>
            </tr>
          </thead>
          <tbody>
            {topViewed.map((p, i) => (
              <tr key={p.productId}>
                <td>{i + 1}</td>
                <td>{p.productName}</td>
                <td>{p.count}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
