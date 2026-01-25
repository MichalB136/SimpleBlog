import { useState } from 'react';
import { imagesApi } from '@/api/images';

interface Props {
  productId: string | null;
  onUploaded: (imageUrl: string) => void;
  disabled?: boolean;
}

export function ProductThumbnailUpload({ productId, onUploaded, disabled }: Props) {
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    setError(null);
    const file = e.target.files && e.target.files[0];
    if (!file) return;
    if (!productId) {
      setError('Zapisz produkt najpierw, aby przesłać zdjęcie');
      return;
    }
    if (file.size > 10 * 1024 * 1024) {
      setError('Rozmiar pliku nie może przekraczać 10 MB');
      return;
    }

    setUploading(true);
    try {
      const updated = await imagesApi.uploadProductImage(productId, file);
      // try to extract imageUrl from response
      const imageUrl = (updated && (updated.imageUrl || updated.imageUrl === '' ? updated.imageUrl : updated) ) as string;
      onUploaded(imageUrl);
    } catch (err: any) {
      setError(err?.message || 'Błąd podczas przesyłania zdjęcia');
    } finally {
      setUploading(false);
      // clear file input value
      if (e.target) e.target.value = '';
    }
  };

  return (
    <div className="mt-2">
      <label className="form-label d-block">Prześlij zdjęcie produktu</label>
      <input type="file" accept="image/*" className="form-control" onChange={handleChange} disabled={disabled || uploading} />
      {uploading && <div className="small text-muted mt-1">Przesyłanie...</div>}
      {error && <div className="text-danger small mt-1">{error}</div>}
    </div>
  );
}

export default ProductThumbnailUpload;
