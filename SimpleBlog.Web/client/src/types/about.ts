export interface About {
  id: string;
  content: string;
  updatedAt: string;
  updatedBy: string;
}

export interface UpdateAboutRequest {
  content: string;
}
