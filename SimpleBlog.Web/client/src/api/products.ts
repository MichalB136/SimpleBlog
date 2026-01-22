import { apiClient } from './client';
import type { Product, CreateProductRequest, UpdateProductRequest, PaginatedResponse } from '@/types/product';

export const productsApi = {
  getAll: (page: number = 1, pageSize: number = 10, filter?: { tagIds?: string[], category?: string, searchTerm?: string }) => {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    if (filter?.tagIds && filter.tagIds.length > 0) {
      filter.tagIds.forEach(id => params.append('tagIds', id));
    }
    if (filter?.category) {
      params.append('category', filter.category);
    }
    if (filter?.searchTerm) {
      params.append('searchTerm', filter.searchTerm);
    }
    return apiClient.get<PaginatedResponse<Product>>(`/products?${params.toString()}`);
  },
  getById: (id: string) => apiClient.get<Product>(`/products/${id}`),
  create: (request: CreateProductRequest) => apiClient.post<Product>('/products', request),
  update: (id: string, request: UpdateProductRequest) => apiClient.put<Product>(`/products/${id}`, request),
  delete: (id: string) => apiClient.delete<void>(`/products/${id}`),
  assignTags: (productId: string, tagIds: string[]) =>
    apiClient.put<Product>(`/products/${productId}/tags`, { tagIds }),
};
