import { useEffect, useState } from 'react';

type TopProduct = {
  productId: string;
  productName: string;
  count: number;
};

export function Analytics() {
  const [topSold, setTopSold] = useState<TopProduct[]>([]);
  const [topViewed, setTopViewed] = useState<TopProduct[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function load() {
      setLoading(true);
      setError(null);
      try {
        const token = localStorage.getItem('authToken');
        const headers: any = { 'Content-Type': 'application/json' };
        if (token) headers.Authorization = `Bearer ${token}`;

        const [soldRes, viewedRes] = await Promise.all([
          fetch('/products/analytics/top-sold?limit=10', { headers }),
          fetch('/products/analytics/top-viewed?limit=10', { headers })
        ]);

        if (!soldRes.ok || !viewedRes.ok) {
          if (soldRes.status === 401 || viewedRes.status === 401) {
            setError('Brak autoryzacji: zaloguj się jako administrator');
          } else {
            setError('Błąd podczas pobierania danych analitycznych');
          }
          setLoading(false);
          return;
        }

        const soldJson = await soldRes.json();
        const viewedJson = await viewedRes.json();

        setTopSold(soldJson as TopProduct[]);
        setTopViewed(viewedJson as TopProduct[]);
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
