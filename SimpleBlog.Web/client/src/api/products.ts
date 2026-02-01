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
  create: (request: CreateProductRequest, files?: File[]) => {
    if (files && files.length > 0) {
      const formData = new FormData();
      formData.append('name', request.name);
      formData.append('description', request.description);
      formData.append('price', request.price.toString());
      formData.append('stock', request.stock.toString());
      if (request.imageUrl) formData.append('imageUrl', request.imageUrl);
      if (request.category) formData.append('category', request.category);
      
      files.forEach(file => formData.append('images', file));
      
      return apiClient.post<Product>('/products', formData);
    }
    
    return apiClient.post<Product>('/products', request);
  },
  update: (id: string, request: UpdateProductRequest) => apiClient.put<Product>(`/products/${id}`, request),
  delete: (id: string) => apiClient.delete<void>(`/products/${id}`),
  assignTags: (productId: string, tagIds: string[]) =>
    apiClient.put<Product>(`/products/${productId}/tags`, { tagIds }),
};
