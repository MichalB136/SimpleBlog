import { useState, useCallback, useEffect } from 'react';
import type { About } from '@/types/about';
import { aboutApi } from '@/api/about';

export function useAbout() {
  const [about, setAbout] = useState<About | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const refresh = useCallback(async () => {
    setError('');
    try {
      const data = await aboutApi.get();
      setAbout(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load about');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const update = useCallback(
    async (content: string) => {
      try {
        const updated = await aboutApi.update({ content });
        setAbout(updated);
        return updated;
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to update about');
        throw err;
      }
    },
    []
  );

  return { about, loading, error, refresh, update, setError };
}
