import React, { useState, useEffect } from 'react';
import type { Post } from '@/types/post';
import { TagSelector } from '@/components/common/TagSelector';
import { ImageManager } from './ImageManager';

interface PostFormProps {
  post?: Post | null;
  onSubmit: (
    data: { title: string; content: string; author: string },
    files?: File[],
    tagIds?: string[]
  ) => Promise<void>;
  onCancel: () => void;
  onAddImage?: (postId: string, file: File) => Promise<void>;
  onRemoveImage?: (postId: string, imageUrl: string) => Promise<void>;
}

export function PostForm({ post, onSubmit, onCancel, onAddImage, onRemoveImage }: PostFormProps) {
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [author, setAuthor] = useState('');
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [previewUrls, setPreviewUrls] = useState<string[]>([]);
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [selectedTagIds, setSelectedTagIds] = useState<string[]>([]);

  useEffect(() => {
    if (post) {
      setTitle(post.title);
      setContent(post.content);
      setAuthor(post.author);
      setSelectedTagIds(post.tags?.map((t) => t.id) ?? []);
    } else {
      setTitle('');
      setContent('');
      setAuthor('');
      setSelectedTagIds([]);
    }
    setError('');
    
    // Cleanup preview URLs
    previewUrls.forEach(url => URL.revokeObjectURL(url));
    setSelectedFiles([]);
    setPreviewUrls([]);

    // Cleanup on unmount
    return () => {
      previewUrls.forEach(url => URL.revokeObjectURL(url));
    };
  }, [post, previewUrls]);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    
    // Validate file size
    const maxSize = 10 * 1024 * 1024; // 10 MB
    const invalidFiles = files.filter(f => f.size > maxSize);
    if (invalidFiles.length > 0) {
      setError(`Niektóre pliki przekraczają limit 10 MB: ${invalidFiles.map(f => f.name).join(', ')}`);
      return;
    }

    // Validate file types
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    const invalidTypes = files.filter(f => !allowedTypes.includes(f.type));
    if (invalidTypes.length > 0) {
      setError(`Niektóre pliki mają nieprawidłowy typ: ${invalidTypes.map(f => f.name).join(', ')}`);
      return;
    }

    setSelectedFiles(files);
    
    // Create local previews
    const previews = files.map(file => URL.createObjectURL(file));
    setPreviewUrls(previews);
    setError('');
  };

  const handleRemovePreview = (index: number) => {
    URL.revokeObjectURL(previewUrls[index]);
    setSelectedFiles(prev => prev.filter((_, i) => i !== index));
    setPreviewUrls(prev => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim() || !content.trim() || !author.trim()) {
      setError('Wszystkie pola są wymagane');
      return;
    }

    setIsSubmitting(true);
    setError('');

    try {
      if (post) {
        // Editing existing post - send selected tags
        await onSubmit({ title: title.trim(), content: content.trim(), author: author.trim() }, undefined, selectedTagIds);
      } else {
        // Creating new post - include files + tags
        await onSubmit(
          { title: title.trim(), content: content.trim(), author: author.trim() },
          selectedFiles.length > 0 ? selectedFiles : undefined,
          selectedTagIds
        );
      }
      
      // Cleanup previews on success
      previewUrls.forEach(url => URL.revokeObjectURL(url));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Błąd podczas zapisywania posta');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <>
      <div className="modal-backdrop show"></div>
      <div className="modal show d-block" tabIndex={-1}>
        <div className="modal-dialog modal-lg">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title">
                {post ? 'Edytuj post' : 'Nowy post'}
              </h5>
              <button
                type="button"
                className="btn-close"
                onClick={onCancel}
                disabled={isSubmitting}
              ></button>
            </div>
            <div className="modal-body">
              <form id="post-form" onSubmit={handleSubmit}>
                <div className="mb-3">
                  <label className="form-label">Tytuł *</label>
                  <input
                    className="form-control"
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                    placeholder="Tytuł posta"
                    required
                    disabled={isSubmitting}
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Autor *</label>
                  <input
                    className="form-control"
                    value={author}
                    onChange={(e) => setAuthor(e.target.value)}
                    placeholder="Autor posta"
                    required
                    disabled={isSubmitting}
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Treść *</label>
                  <textarea
                    className="form-control"
                    rows={8}
                    value={content}
                    onChange={(e) => setContent(e.target.value)}
                    placeholder="Treść posta..."
                    required
                    disabled={isSubmitting}
                  ></textarea>
                </div>

                <div className="mb-3">
                  <label className="form-label">Tagi (style, materiały, okazje)</label>
                  <TagSelector
                    selectedTagIds={selectedTagIds}
                    onChange={setSelectedTagIds}
                    disabled={isSubmitting}
                  />
                  <small className="text-muted">Wybierz tagi, aby łatwiej filtrować artykuły o modzie.</small>
                </div>
                
                {!post && (
                  <div className="mb-3">
                    <label className="form-label">Zdjęcia (opcjonalne)</label>
                    <input
                      type="file"
                      className="form-control"
                      accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                      multiple
                      onChange={handleFileSelect}
                      disabled={isSubmitting}
                    />
                    <small className="text-muted">
                      Maksymalnie 10 MB na plik. Formaty: JPEG, PNG, GIF, WebP
                    </small>
                    
                    {previewUrls.length > 0 && (
                      <div className="mt-3">
                        <h6>Podgląd ({previewUrls.length} {previewUrls.length === 1 ? 'zdjęcie' : 'zdjęć'}):</h6>
                        <div className="row g-2">
                          {previewUrls.map((url, index) => (
                            <div key={index} className="col-4 position-relative">
                              <img 
                                src={url} 
                                alt={`Podgląd ${index + 1}`} 
                                className="img-thumbnail w-100" 
                                style={{ height: '150px', objectFit: 'cover' }}
                              />
                              <button
                                type="button"
                                className="btn btn-sm btn-danger position-absolute top-0 end-0 m-1"
                                onClick={() => handleRemovePreview(index)}
                                disabled={isSubmitting}
                                title="Usuń zdjęcie"
                              >
                                <i className="bi bi-x"></i>
                              </button>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}
                  </div>
                )}
                
                {error && <div className="alert alert-danger">{error}</div>}
              </form>
              
              {post && onAddImage && onRemoveImage && (
                <>
                  <hr className="my-3" />
                  <ImageManager
                    post={post}
                    onAddImage={async (file) => {
                      await onAddImage(post.id, file);
                    }}
                    onRemoveImage={async (imageUrl) => {
                      await onRemoveImage(post.id, imageUrl);
                    }}
                  />
                </>
              )}
            </div>
            <div className="modal-footer">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={onCancel}
                disabled={isSubmitting}
              >
                Anuluj
              </button>
              <button
                type="submit"
                form="post-form"
                className="btn btn-primary"
                disabled={isSubmitting}
              >
                {isSubmitting ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2"></span>
                    Zapisywanie...
                  </>
                ) : (
                  <>
                    <i className="bi bi-save me-2"></i>
                    {post ? 'Zapisz zmiany' : 'Utwórz post'}
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
