import React from 'react';
import { useNavigate } from 'react-router-dom';
import type { Post } from '@/types/post';
import { TagBadges } from '@/components/common/TagSelector';
import { CommentForm } from './CommentForm';

interface PostListProps {
  posts: Post[];
  isAdmin: boolean;
  onDelete?: (id: string) => void;
  onEdit?: (post: Post) => void;
  onAddComment?: (postId: string, payload: any) => void;
  onTogglePin?: (post: Post) => void;
}

export function PostList({ posts, isAdmin, onDelete, onEdit, onAddComment, onTogglePin }: PostListProps) {
  const navigate = useNavigate();
  const [selectedPost, setSelectedPost] = React.useState<Post | null>(null);

  if (!posts.length) {
    return <div className="alert alert-info">Brak wpisów. Zacznij od pierwszego!</div>;
  }

  const handlePinClick = (e: React.MouseEvent, post: Post) => {
    e.stopPropagation();
    onTogglePin?.(post);
  };

  return (
    <>
      <div className="row g-4">
        {posts.map((post) => (
          <div key={post.id} className="col-12">
            <div
              className="card shadow-sm"
              style={{ cursor: 'pointer' }}
              onClick={() => setSelectedPost(post)}
            >
              <div className="row g-0">
                {/* Image column - 40% on desktop, full width on mobile */}
                <div className="col-md-5 col-lg-4">
                  {post.imageUrls && post.imageUrls.length > 0 ? (
                    <div 
                      className="position-relative rounded-start overflow-hidden"
                      style={{ height: '300px', width: '100%' }}
                    >
                      {/* Blurred background */}
                      <div
                        className="position-absolute w-100 h-100"
                        style={{
                          backgroundImage: `url(${post.imageUrls[0]})`,
                          backgroundSize: 'cover',
                          backgroundPosition: 'center',
                          filter: 'blur(20px)',
                          transform: 'scale(1.1)'
                        }}
                      />
                      {/* Main image */}
                      <img
                        src={post.imageUrls[0]}
                        className="position-relative img-fluid"
                        style={{
                          height: '300px',
                          width: '100%',
                          objectFit: 'contain',
                          zIndex: 1
                        }}
                        alt={post.title}
                      />
                    </div>
                  ) : (
                    <div
                      className="bg-light d-flex align-items-center justify-content-center rounded-start"
                      style={{ height: '300px', width: '100%' }}
                    >
                      <i className="bi bi-image text-muted" style={{ fontSize: '3rem' }}></i>
                    </div>
                  )}
                </div>

                {/* Text column - 60% on desktop, full width on mobile */}
                <div className="col-md-7 col-lg-8">
                  <div className="card-body d-flex flex-column h-100">
                    <div className="d-flex justify-content-between align-items-start mb-2">
                      <div style={{ flex: 1 }}>
                        <small className="text-muted d-block">
                          <i className="bi bi-clock me-1"></i>
                          {new Date(post.createdAt).toLocaleString()}
                        </small>
                        <h5 className="card-title mb-1 d-flex align-items-center gap-2">
                          {post.title}
                          {post.isPinned && (
                            <span className="badge bg-warning text-dark">
                              <i className="bi bi-pin-angle-fill me-1"></i>Przypięty
                            </span>
                          )}
                        </h5>
                        <small className="text-muted">
                          <i className="bi bi-person me-1"></i>{post.author || 'Anon'}
                        </small>
                        {post.tags && post.tags.length > 0 && (
                          <div className="mt-2">
                            <TagBadges tags={post.tags} />
                          </div>
                        )}
                      </div>
                      {isAdmin && (
                        <div className="btn-group">
                          <button
                            className={post.isPinned ? 'btn btn-sm btn-warning text-dark' : 'btn btn-sm btn-outline-warning'}
                            onClick={(e) => handlePinClick(e, post)}
                            title={post.isPinned ? 'Odepnij' : 'Przytnij'}
                          >
                            <i className={post.isPinned ? 'bi bi-pin-angle-fill' : 'bi bi-pin-angle'}></i>
                          </button>
                          <button
                            className="btn btn-sm btn-outline-primary"
                            onClick={(e) => {
                              e.stopPropagation();
                              onEdit?.(post);
                            }}
                            title="Edytuj"
                          >
                            <i className="bi bi-pencil"></i>
                          </button>
                          <button
                            className="btn btn-sm btn-outline-danger"
                            onClick={(e) => {
                              e.stopPropagation();
                              onDelete?.(post.id);
                            }}
                            title="Usuń"
                          >
                            <i className="bi bi-trash"></i>
                          </button>
                        </div>
                      )}
                    </div>
                    <p className="card-text flex-grow-1" style={{ whiteSpace: 'pre-wrap' }}>
                      {post.content.substring(0, 150)}...
                    </p>
                    <div className="d-flex justify-content-between align-items-center mt-auto">
                      <span className="text-muted small">
                        <i className="bi bi-chat-dots me-1"></i>
                        {post.comments?.length ?? 0} {post.comments?.length === 1 ? 'komentarz' : 'komentarzy'}
                      </span>
                      <span className="btn btn-link btn-sm p-0">
                        Czytaj więcej <i className="bi bi-arrow-right"></i>
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>

      {selectedPost && (
        <>
          <div className="modal-backdrop show" onClick={() => setSelectedPost(null)}></div>
          <div className="modal show d-block" tabIndex={-1} style={{ overflowY: 'auto' }}>
            <div className="modal-dialog modal-xl modal-dialog-scrollable">
              <div
                className="modal-content"
                onClick={(e) => e.stopPropagation()}
              >
                <div className="modal-header">
                  <div>
                    <h3 className="modal-title">{selectedPost.title}</h3>
                    <div className="mt-2">
                      <small className="text-muted me-3">
                        <i className="bi bi-clock me-1"></i>
                        {new Date(selectedPost.createdAt).toLocaleString()}
                      </small>
                      <small className="text-muted">
                        <i className="bi bi-person me-1"></i>
                        {selectedPost.author || 'Anon'}
                      </small>
                    </div>
                  </div>
                  <div className="d-flex gap-2">
                    <button
                      type="button"
                      className="btn btn-sm btn-outline-primary"
                      onClick={() => {
                        const url = `${window.location.origin}/posts/${selectedPost.id}`;
                        navigator.clipboard.writeText(url);
                        alert('Link skopiowany do schowka!');
                      }}
                      title="Udostępnij link"
                    >
                      <i className="bi bi-share"></i>
                    </button>
                    <button
                      type="button"
                      className="btn btn-sm btn-outline-info"
                      onClick={() => {
                        setSelectedPost(null);
                        navigate(`/posts/${selectedPost.id}`);
                      }}
                      title="Otwórz pełny widok"
                    >
                      <i className="bi bi-box-arrow-up-right"></i>
                    </button>
                    <button
                      type="button"
                      className="btn-close"
                      onClick={() => setSelectedPost(null)}
                      aria-label="Close"
                    ></button>
                  </div>
                </div>
                <div className="modal-body">
                  <div className="row g-0">
                    {/* Image column - 40% on desktop, full width on mobile */}
                    {selectedPost.imageUrls && selectedPost.imageUrls.length > 0 && (
                      <div className="col-md-5 col-lg-4 mb-4 mb-md-0">
                        <div className="sticky-top" style={{ top: '20px' }}>
                          {selectedPost.imageUrls.map((url, index) => (
                            <div
                              key={index}
                              className="position-relative rounded overflow-hidden mb-3"
                              style={{ height: '300px', width: '100%' }}
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
                                  height: '300px',
                                  width: '100%',
                                  objectFit: 'contain',
                                  zIndex: 1
                                }}
                                alt={`${selectedPost.title} - zdjęcie ${index + 1}`}
                              />
                            </div>
                          ))}
                        </div>
                      </div>
                    )}
                    
                    {/* Text column - 60% on desktop, full width on mobile */}
                    <div className={selectedPost.imageUrls && selectedPost.imageUrls.length > 0 ? 'col-md-7 col-lg-8' : 'col-12'}>
                      <div className={selectedPost.imageUrls && selectedPost.imageUrls.length > 0 ? 'ps-md-4' : ''}>
                        {selectedPost.tags && selectedPost.tags.length > 0 && (
                          <div className="mb-3">
                            <TagBadges tags={selectedPost.tags} />
                          </div>
                        )}
                        <p className="fs-5 lh-lg" style={{ whiteSpace: 'pre-wrap' }}>
                          {selectedPost.content}
                        </p>
                      </div>
                    </div>
                  </div>
                  
                  <hr className="my-4" />
                  <div>
                    <h4 className="mb-4">
                      <i className="bi bi-chat-dots me-2"></i>
                      Komentarze ({selectedPost.comments?.length ?? 0})
                    </h4>
                    {selectedPost.comments?.length ? (
                      <div className="mb-4">
                        {selectedPost.comments.map((c) => (
                          <div key={c.id} className="card mb-3">
                            <div className="card-body">
                              <div className="d-flex justify-content-between mb-2">
                                <strong>{c.author || 'Anon'}</strong>
                                <small className="text-muted">{new Date(c.createdAt).toLocaleString()}</small>
                              </div>
                              <p className="mb-0">{c.content}</p>
                            </div>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <p className="text-muted mb-4">Brak komentarzy. Dodaj pierwszy!</p>
                    )}
                    <CommentForm onAdd={(payload) => onAddComment?.(selectedPost.id, payload)} />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </>
      )}
    </>
  );
}
