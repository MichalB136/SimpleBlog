export interface Post {
  id: string;
  title: string;
  content: string;
  author: string;
  createdAt: string;
  imageUrls: string[];
  isPinned: boolean;
  comments?: Comment[];
}

export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface CreatePostRequest {
  title: string;
  content: string;
  author: string;
}

export interface UpdatePostRequest {
  title: string;
  content: string;
  author: string;
}

export interface Comment {
  id: string;
  postId: string;
  author: string;
  content: string;
  createdAt: string;
}

export interface CreateCommentRequest {
  author: string;
  content: string;
}
