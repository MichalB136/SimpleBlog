import { apiClient } from './client';

export const imagesApi = {
  uploadProductImage: async (productId: string, file: File) => {
    const form = new FormData();
    form.append('file', file);
    // returns updated product object from API
    return apiClient.post<any>(`/products/${productId}/images`, form);
  },
};

export default imagesApi;
