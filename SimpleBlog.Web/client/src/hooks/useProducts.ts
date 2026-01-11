import { useState, useCallback, useEffect } from 'react';
import type { Product } from '@/types/product';
import { productsApi } from '@/api/products';

export function useProducts() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const refresh = useCallback(async () => {
    setError('');
    try {
      const data = await productsApi.getAll();
      setProducts(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load products');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const create = useCallback(
    async (payload: any) => {
      try {
        await productsApi.create(payload);
        await refresh();
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to create product');
        throw err;
      }
    },
    [refresh]
  );

  const update = useCallback(
    async (id: string, payload: any) => {
      try {
        await productsApi.update(id, payload);
        await refresh();
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to update product');
        throw err;
      }
    },
    [refresh]
  );

  const delete_ = useCallback(
    async (id: string) => {
      try {
        await productsApi.delete(id);
        setProducts((prev) => prev.filter((p) => p.id !== id));
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to delete product');
        throw err;
      }
    },
    []
  );

  return { products, loading, error, refresh, create, update, delete: delete_, setError };
}
