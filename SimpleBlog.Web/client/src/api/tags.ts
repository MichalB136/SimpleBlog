import { apiClient } from './client';

export interface Tag {
  id: string;
  name: string;
  slug: string;
  color?: string;
  createdAt: string;
}

export interface CreateTagRequest {
  name: string;
  color?: string;
}

export interface UpdateTagRequest {
  name: string;
  color?: string;
}

export interface AssignTagsRequest {
  tagIds: string[];
}

class TagsApi {
  async getAll(): Promise<Tag[]> {
    return apiClient.get<Tag[]>('/tags');
  }

  async getById(id: string): Promise<Tag> {
    return apiClient.get<Tag>(`/tags/${id}`);
  }

  async getBySlug(slug: string): Promise<Tag> {
    return apiClient.get<Tag>(`/tags/by-slug/${slug}`);
  }

  async create(request: CreateTagRequest): Promise<Tag> {
    return apiClient.post<Tag>('/tags', request);
  }

  async update(id: string, request: UpdateTagRequest): Promise<Tag> {
    return apiClient.put<Tag>(`/tags/${id}`, request);
  }

  async delete(id: string): Promise<void> {
    return apiClient.delete(`/tags/${id}`);
  }

  async assignToPost(postId: string, request: AssignTagsRequest): Promise<void> {
    return apiClient.put(`/posts/${postId}/tags`, request);
  }

  async assignToProduct(productId: string, request: AssignTagsRequest): Promise<void> {
    return apiClient.put(`/products/${productId}/tags`, request);
  }
}

export const tagsApi = new TagsApi();
