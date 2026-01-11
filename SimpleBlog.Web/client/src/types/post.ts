export interface Post {
  id: string;
  title: string;
  content: string;
  author: string;
  createdAt: string;
  imageUrl?: string;
  isPinned: boolean;
  comments?: Comment[];
}

export interface CreatePostRequest {
  title: string;
  content: string;
  author: string;
  imageUrl?: string;
}

export interface UpdatePostRequest {
  title: string;
  content: string;
  author: string;
  imageUrl?: string;
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
