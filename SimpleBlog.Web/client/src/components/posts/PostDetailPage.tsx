import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import type { Post } from '@/types/post';
import { TagBadges } from '@/components/common/TagSelector';
import { CommentForm } from './CommentForm';

interface PostDetailPageProps {
  posts: Post[];
  isAdmin: boolean;
  onDelete?: (id: string) => void;
  onEdit?: (post: Post) => void;
  onAddComment?: (postId: string, payload: any) => void;
  onTogglePin?: (post: Post) => void;
}

export function PostDetailPage({
  posts,
  isAdmin,
  onDelete,
  onEdit,
  onAddComment,
  onTogglePin
}: PostDetailPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const post = React.useMemo(
    () => id ? posts.find(p => p.id === id) ?? null : null,
    [id, posts]
  );

  if (!post) {
    return (
      <div className="container my-5">
        <div className="alert alert-warning">
          <i className="bi bi-exclamation-triangle me-2"></i>
          Post nie znaleziony
        </div>
        <button 
          className="btn btn-outline-secondary"
          onClick={() => navigate('/')}
        >
          <i className="bi bi-arrow-left me-2"></i>
          Powrót do listy postów
        </button>
      </div>
    );
  }

  const handlePinClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    onTogglePin?.(post);
  };

  const copyPostLink = () => {
    const url = `${window.location.origin}/posts/${post.id}`;
    navigator.clipboard.writeText(url);
  };

  return (
    <div className="container my-5">
      <div className="mb-4">
        <button 
          className="btn btn-outline-secondary me-2"
          onClick={() => navigate('/')}
        >
          <i className="bi bi-arrow-left me-2"></i>
          Powrót
        </button>
        {isAdmin && (
          <>
            <button
              className={post.isPinned ? 'btn btn-warning text-dark me-2' : 'btn btn-outline-warning me-2'}
              onClick={handlePinClick}
              title={post.isPinned ? 'Odepnij' : 'Przytnij'}
            >
              <i className={post.isPinned ? 'bi bi-pin-angle-fill me-1' : 'bi bi-pin-angle me-1'}></i>
              {post.isPinned ? 'Odepnij' : 'Przytnij'}
            </button>
            <button
              className="btn btn-outline-primary me-2"
              onClick={() => onEdit?.(post)}
              title="Edytuj"
            >
              <i className="bi bi-pencil me-1"></i>
              Edytuj
            </button>
            <button
              className="btn btn-outline-danger me-2"
              onClick={() => {
                if (confirm('Czy na pewno chcesz usunąć ten post?')) {
                  onDelete?.(post.id);
                  navigate('/');
                }
              }}
              title="Usuń"
            >
              <i className="bi bi-trash me-1"></i>
              Usuń
            </button>
          </>
        )}
        <button
          className="btn btn-outline-primary"
          onClick={() => {
            copyPostLink();
            alert('Link skopiowany do schowka!');
          }}
          title="Udostępnij link"
        >
          <i className="bi bi-share me-1"></i>
          Udostępnij
        </button>
      </div>

      <div className="row">
        {/* Image column - 40% on desktop, full width on mobile */}
        {post.imageUrls && post.imageUrls.length > 0 && (
          <div className="col-md-5 col-lg-4 mb-4 mb-md-0">
            <div className="sticky-top" style={{ top: '20px' }}>
              {post.imageUrls.map((url, index) => (
                <div
                  key={index}
                  className="position-relative rounded overflow-hidden mb-3"
                  style={{ height: '400px', width: '100%' }}
                >
                  {/* Blurred background */}
                  <div
                    className="position-absolute w-100 h-100"
                    style={{
                      backgroundImage: `url(${url})`,
                      backgroundSize: 'cover',
                      backgroundPosition: 'center',
                      filter: 'blur(20px)',
                      transform: 'scale(1.1)'
                    }}
                  />
                  {/* Main image */}
                  <img
                    src={url}
                    className="position-relative img-fluid"
                    style={{
                      height: '400px',
                      width: '100%',
                      objectFit: 'contain',
                      zIndex: 1
                    }}
                    alt={`${post.title} - zdjęcie ${index + 1}`}
                  />
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Text column - 60% on desktop, full width on mobile */}
        <div className={post.imageUrls && post.imageUrls.length > 0 ? 'col-md-7 col-lg-8' : 'col-12'}>
          <div className={post.imageUrls && post.imageUrls.length > 0 ? 'ps-md-4' : ''}>
            <h1 className="mb-2">{post.title}</h1>
            
            <div className="d-flex align-items-center mb-3 text-muted">
              <i className="bi bi-clock me-2"></i>
              <span className="me-3">{new Date(post.createdAt).toLocaleString()}</span>
              <i className="bi bi-person me-2"></i>
              <span>{post.author || 'Anon'}</span>
              {post.isPinned && (
                <>
                  <span className="ms-3">
                    <span className="badge bg-warning text-dark">
                      <i className="bi bi-pin-angle-fill me-1"></i>
                      Przypięty
                    </span>
                  </span>
                </>
              )}
            </div>

            {post.tags && post.tags.length > 0 && (
              <div className="mb-4">
                <TagBadges tags={post.tags} />
              </div>
            )}

            <div className="fs-5 lh-lg mb-4" style={{ whiteSpace: 'pre-wrap' }}>
              {post.content}
            </div>

            <hr className="my-4" />

            {/* Comments section */}
            <div>
              <h4 className="mb-4">
                <i className="bi bi-chat-dots me-2"></i>
                Komentarze ({post.comments?.length ?? 0})
              </h4>
              
              {post.comments?.length ? (
                <div className="mb-4">
                  {post.comments.map((c) => (
                    <div key={c.id} className="card mb-3">
                      <div className="card-body">
                        <div className="d-flex justify-content-between mb-2">
                          <strong>{c.author || 'Anon'}</strong>
                          <small className="text-muted">
                            {new Date(c.createdAt).toLocaleString()}
                          </small>
                        </div>
                        <p className="mb-0">{c.content}</p>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-muted mb-4">Brak komentarzy. Dodaj pierwszy!</p>
              )}

              <CommentForm 
                onAdd={(payload) => {
                  onAddComment?.(post.id, payload);
                }} 
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
