import { useState, useCallback, useEffect } from 'react';
import type { About } from '@/types/about';
import { aboutApi } from '@/api/about';
import { siteSettingsApi } from '@/api/siteSettings';

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
    async (content: string, imageUrl?: string | null) => {
      try {
        const updated = await aboutApi.update({ content, imageUrl: imageUrl ?? null });
        setAbout(updated);
        return updated;
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to update about');
        throw err;
      }
    },
    []
  );

  const uploadImage = useCallback(async (file: File) => {
    try {
      setLoading(true);
      const updated = await siteSettingsApi.uploadAboutImage(file);
      setAbout(updated);
      return { success: true };
    } catch (err: any) {
      setError(err.message || 'Failed to upload image');
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  }, []);

  const deleteImage = useCallback(async () => {
    try {
      setLoading(true);
      const updated = await siteSettingsApi.deleteAboutImage();
      setAbout(updated);
      return { success: true };
    } catch (err: any) {
      setError(err.message || 'Failed to delete image');
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  }, []);

  return { about, loading, error, refresh, update, uploadImage, deleteImage, setError };
}
