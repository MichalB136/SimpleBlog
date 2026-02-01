export interface About {
  id: string;
  content: string;
  imageUrl?: string | null;
  updatedAt: string;
  updatedBy: string;
}

export interface UpdateAboutRequest {
  content: string;
  imageUrl?: string | null;
}
