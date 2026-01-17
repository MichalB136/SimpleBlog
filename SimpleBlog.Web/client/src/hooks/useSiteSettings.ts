import { useState, useEffect } from 'react';
import { siteSettingsApi, SiteSettings } from '@/api/siteSettings';

export function useSiteSettings() {
  const [settings, setSettings] = useState<SiteSettings | null>(null);
  const [availableThemes, setAvailableThemes] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchSettings = async () => {
    try {
      setLoading(true);
      const data = await siteSettingsApi.get();
      setSettings(data);
      
      // Apply theme to document
      if (data.theme) {
        applyTheme(data.theme);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to fetch site settings');
    } finally {
      setLoading(false);
    }
  };

  const fetchAvailableThemes = async () => {
    try {
      const themes = await siteSettingsApi.getAvailableThemes();
      setAvailableThemes(themes);
    } catch (err: any) {
      console.error('Failed to fetch available themes:', err);
    }
  };

  const updateTheme = async (theme: string) => {
    try {
      setLoading(true);
      const data = await siteSettingsApi.update({ theme });
      setSettings(data);
      applyTheme(theme);
      return { success: true };
    } catch (err: any) {
      setError(err.message || 'Failed to update theme');
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  };

  const applyTheme = (theme: string) => {
    // Remove all theme classes
    document.documentElement.className = document.documentElement.className
      .split(' ')
      .filter((cls) => !cls.startsWith('theme-'))
      .join(' ');
    
    // Add new theme class
    document.documentElement.classList.add(`theme-${theme}`);
    
    // Save to localStorage
    localStorage.setItem('site-theme', theme);
  };

  const uploadLogo = async (file: File) => {
    try {
      setLoading(true);
      const data = await siteSettingsApi.uploadLogo(file);
      setSettings(data);
      return { success: true };
    } catch (err: any) {
      setError(err.message || 'Failed to upload logo');
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  };

  const deleteLogo = async () => {
    try {
      setLoading(true);
      const data = await siteSettingsApi.deleteLogo();
      setSettings(data);
      return { success: true };
    } catch (err: any) {
      setError(err.message || 'Failed to delete logo');
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSettings();
    fetchAvailableThemes();
  }, []);

  return {
    settings,
    availableThemes,
    loading,
    error,
    updateTheme,
    uploadLogo,
    deleteLogo,
    refresh: fetchSettings,
  };
}
