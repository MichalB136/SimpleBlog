import { apiClient } from './client';
import type { Product, CreateProductRequest, UpdateProductRequest } from '@/types/product';

export const productsApi = {
  getAll: () => apiClient.get<Product[]>('/products'),
  getById: (id: string) => apiClient.get<Product>(`/products/${id}`),
  create: (request: CreateProductRequest) => apiClient.post<Product>('/products', request),
  update: (id: string, request: UpdateProductRequest) => apiClient.put<Product>(`/products/${id}`, request),
  delete: (id: string) => apiClient.delete<void>(`/products/${id}`),
};
