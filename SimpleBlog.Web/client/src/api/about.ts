import { apiClient } from './client';
import type { About, UpdateAboutRequest } from '@/types/about';

export const aboutApi = {
  get: () => apiClient.get<About>('/aboutme'),
  update: (request: UpdateAboutRequest) => apiClient.put<About>('/aboutme', request),
};
