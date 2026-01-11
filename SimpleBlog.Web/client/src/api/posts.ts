import { apiClient } from './client';
import type { Post, CreatePostRequest, UpdatePostRequest, Comment, CreateCommentRequest, PaginatedResponse } from '@/types/post';

export const postsApi = {
  getAll: (page: number = 1, pageSize: number = 10) =>
    apiClient.get<PaginatedResponse<Post>>(`/posts?page=${page}&pageSize=${pageSize}`),
  getById: (id: string) => apiClient.get<Post>(`/posts/${id}`),
  create: (request: CreatePostRequest) => apiClient.post<Post>('/posts', request),
  update: (id: string, request: UpdatePostRequest) => apiClient.put<Post>(`/posts/${id}`, request),
  delete: (id: string) => apiClient.delete<void>(`/posts/${id}`),
  pin: (id: string) => apiClient.put<Post>(`/posts/${id}/pin`),
  unpin: (id: string) => apiClient.put<Post>(`/posts/${id}/unpin`),
  getComments: (id: string) => apiClient.get<Comment[]>(`/posts/${id}/comments`),
  addComment: (postId: string, request: CreateCommentRequest) =>
    apiClient.post<Comment>(`/posts/${postId}/comments`, request),
};
