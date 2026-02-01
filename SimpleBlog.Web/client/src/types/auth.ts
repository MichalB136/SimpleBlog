export interface User {
  username: string;
  email: string;
  role: 'Admin' | 'User';
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface RequestPasswordResetRequest {
  email: string;
}

export interface PasswordResetRequest {
  userId: string;
  token: string;
  newPassword: string;
}

export interface ConfirmEmailRequest {
  userId: string;
  token: string;
}

export interface SendEmailConfirmationRequest {
  email: string;
}

export interface OperationResponse {
  success: boolean;
  message?: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  email: string;
  role: string;
}
