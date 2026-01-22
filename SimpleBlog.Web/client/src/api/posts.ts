import { apiClient } from './client';
import type { Post, CreatePostRequest, UpdatePostRequest, Comment, CreateCommentRequest, PaginatedResponse } from '@/types/post';

export const postsApi = {
  getAll: (page: number = 1, pageSize: number = 10, filter?: { tagIds?: string[], searchTerm?: string }) => {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    if (filter?.tagIds && filter.tagIds.length > 0) {
      filter.tagIds.forEach(id => params.append('tagIds', id));
    }
    if (filter?.searchTerm) {
      params.append('searchTerm', filter.searchTerm);
    }
    return apiClient.get<PaginatedResponse<Post>>(`/posts?${params.toString()}`);
  },
  getById: (id: string) => apiClient.get<Post>(`/posts/${id}`),
  create: (request: CreatePostRequest, files?: File[]) => {
    if (!files || files.length === 0) {
      // JSON only - no images
      return apiClient.post<Post>('/posts', request);
    }
    
    // Multipart with images
    const formData = new FormData();
    formData.append('title', request.title);
    formData.append('content', request.content);
    formData.append('author', request.author);
    
    files.forEach(file => {
      formData.append('images', file);
    });
    
    return apiClient.post<Post>('/posts', formData);
  },
  update: (id: string, request: UpdatePostRequest) => apiClient.put<Post>(`/posts/${id}`, request),
  delete: (id: string) => apiClient.delete<void>(`/posts/${id}`),
  pin: (id: string) => apiClient.put<Post>(`/posts/${id}/pin`),
  unpin: (id: string) => apiClient.put<Post>(`/posts/${id}/unpin`),
  addImage: (id: string, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return apiClient.post<Post>(`/posts/${id}/images`, formData);
  },
  removeImage: (id: string, imageUrl: string) => 
    apiClient.delete<Post>(`/posts/${id}/images?imageUrl=${encodeURIComponent(imageUrl)}`),
  getComments: (id: string) => apiClient.get<Comment[]>(`/posts/${id}/comments`),
  addComment: (postId: string, request: CreateCommentRequest) =>
    apiClient.post<Comment>(`/posts/${postId}/comments`, request),
  assignTags: (postId: string, tagIds: string[]) =>
    apiClient.put<Post>(`/posts/${postId}/tags`, { tagIds }),
};
