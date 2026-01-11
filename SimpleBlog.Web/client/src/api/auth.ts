import { apiClient } from './client';
import type { LoginRequest, RegisterRequest, AuthResponse } from '@/types/auth';

export const authApi = {
  login: (request: LoginRequest) => apiClient.post<AuthResponse>('/login', request),
  register: (request: RegisterRequest) => apiClient.post<AuthResponse>('/register', request),
};
