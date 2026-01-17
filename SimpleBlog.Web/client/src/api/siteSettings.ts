import { apiClient } from './client';

export interface SiteSettings {
  id: string;
  theme: string;
  logoUrl?: string | null;
  updatedAt: string;
  updatedBy: string;
}

export interface UpdateSiteSettingsRequest {
  theme: string;
}

export const siteSettingsApi = {
  get: () => apiClient.get<SiteSettings>('/site-settings'),
  
  update: (request: UpdateSiteSettingsRequest) =>
    apiClient.put<SiteSettings>('/site-settings', request),
  
  getAvailableThemes: () => apiClient.get<string[]>('/site-settings/themes'),

  uploadLogo: async (file: File): Promise<SiteSettings> => {
    const formData = new FormData();
    formData.append('file', file);
    
    const token = localStorage.getItem('authToken');
    const headers: Record<string, string> = {};
    
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
    
    const response = await fetch('/api/site-settings/logo', {
      method: 'POST',
      headers,
      body: formData,
    });
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || 'Failed to upload logo');
    }
    
    return response.json();
  },

  deleteLogo: () => apiClient.delete<SiteSettings>('/site-settings/logo'),
};
