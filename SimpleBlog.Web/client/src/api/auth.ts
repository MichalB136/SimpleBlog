import { apiClient } from './client';
import type { 
  LoginRequest, 
  RegisterRequest, 
  AuthResponse,
  RequestPasswordResetRequest,
  PasswordResetRequest,
  ConfirmEmailRequest,
  OperationResponse
} from '@/types/auth';

export const authApi = {
  login: (request: LoginRequest) => apiClient.post<AuthResponse>('/login', request),
  register: (request: RegisterRequest) => apiClient.post<AuthResponse>('/register', request),
  requestPasswordReset: (request: RequestPasswordResetRequest) => 
    apiClient.post<OperationResponse>('/request-password-reset', request),
  resetPassword: (request: PasswordResetRequest) => 
    apiClient.post<OperationResponse>('/reset-password', request),
  confirmEmail: (request: ConfirmEmailRequest) => 
    apiClient.post<OperationResponse>('/confirm-email', request),
};
