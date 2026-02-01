import { BrowserRouter, Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import { Header } from '@/components/layout/Header';
import { Navigation } from '@/components/layout/Navigation';
import { usePosts } from '@/hooks/usePosts';
import { PostList } from '@/components/posts/PostList';
import { PostDetailPage } from '@/components/posts/PostDetailPage';
import { PostForm } from '@/components/posts/PostForm';
import { PostSearchBar } from '@/components/posts/PostSearchBar';
import { AboutPage } from '@/components/common/AboutPage';
import { ShopPage } from '@/components/shop/ShopPage';
import ProductDetails from '@/components/shop/ProductDetails';
import { CartPage } from '@/components/shop/CartPage';
import { CheckoutPage } from '@/components/shop/CheckoutPage';
import { ContactPage } from '@/components/common/ContactPage';
import { LoginForm } from '@/components/auth/LoginForm';
import { RegisterForm } from '@/components/auth/RegisterForm';
import { PasswordResetForm } from '@/components/auth/PasswordResetForm';
import { ConfirmEmailForm } from '@/components/auth/ConfirmEmailForm';
import { AdminPanel } from '@/components/admin/AdminPanel';
import { useAuth } from '@/context/AuthContext';
import { useState } from 'react';
import type { Post } from '@/types/post';

function AppContent() {
  const { user } = useAuth();
  const { posts, loading, error, delete: deletePost, addComment, togglePin, create, update, addImage, removeImage, setFilter } = usePosts();
  const navigate = useNavigate();
  const [showPostForm, setShowPostForm] = useState(false);
  const [editingPost, setEditingPost] = useState<Post | null>(null);

  const handleLogout = () => {
    navigate('/');
  };

  const handlePostSubmit = async (
    data: { title: string; content: string; author: string },
    files?: File[],
    tagIds?: string[]
  ) => {
    if (editingPost) {
      await update(editingPost.id, data, tagIds);
    } else {
      await create(data, files, tagIds);
    }
    setShowPostForm(false);
    setEditingPost(null);
  };

  const handlePostCancel = () => {
    setShowPostForm(false);
    setEditingPost(null);
  };

  const handleAddImage = async (postId: string, file: File) => {
    const updated = await addImage(postId, file);
    // Update editing post to reflect new images
    if (editingPost && editingPost.id === postId) {
      setEditingPost(updated);
    }
  };

  const handleRemoveImage = async (postId: string, imageUrl: string) => {
    const updated = await removeImage(postId, imageUrl);
    // Update editing post to reflect removed image
    if (editingPost && editingPost.id === postId) {
      setEditingPost(updated);
    }
  };

  const renderHomePage = () => {
    if (loading) {
      return <p className="text-muted">Ładowanie postów...</p>;
    }
    
    if (error) {
      return <div className="alert alert-danger">{error}</div>;
    }
    
    return (
      <>
        {user?.role === 'Admin' && (
          <div className="mb-4">
            <button
              className="btn btn-primary"
              onClick={() => {
                setEditingPost(null);
                setShowPostForm(true);
              }}
            >
              <i className="bi bi-plus-circle me-2"></i>
              Dodaj nowy post
            </button>
          </div>
        )}
        
        <PostSearchBar onSearch={setFilter} />
        
        <PostList
          posts={posts}
          isAdmin={user?.role === 'Admin'}
          onDelete={deletePost}
          onEdit={(post) => {
            setEditingPost(post);
            setShowPostForm(true);
          }}
          onAddComment={addComment}
          onTogglePin={togglePin}
        />
      </>
    );
  };

  if (!user) {
    return (
      <div className="min-vh-100 d-flex flex-column">
        <Header title="" subtitle="" />
        <div className="container my-4">
          <Routes>
            <Route path="/register" element={
              <>
                <ul className="nav nav-tabs mb-4">
                  <li className="nav-item">
                    <button
                      className="nav-link"
                      onClick={() => navigate('/login')}
                    >
                      Logowanie
                    </button>
                  </li>
                  <li className="nav-item">
                    <button className="nav-link active">
                      Rejestracja
                    </button>
                  </li>
                </ul>
                <RegisterForm onSuccess={() => navigate('/login')} />
              </>
            } />
            <Route path="/reset-password" element={
              <>
                <button
                  className="btn btn-outline-secondary w-100 mb-4"
                  onClick={() => navigate('/login')}
                >
                  <i className="bi bi-arrow-left me-2"></i>
                  Powrót do logowania
                </button>
                <PasswordResetForm 
                  onSuccess={() => navigate('/login')}
                  onCancel={() => navigate('/login')}
                />
              </>
            } />
            <Route path="/confirm-email" element={
              <>
                <button
                  className="btn btn-outline-secondary w-100 mb-4"
                  onClick={() => navigate('/login')}
                >
                  <i className="bi bi-arrow-left me-2"></i>
                  Powrót do logowania
                </button>
                <ConfirmEmailForm 
                  onSuccess={() => navigate('/login')}
                  onCancel={() => navigate('/login')}
                />
              </>
            } />
            <Route path="*" element={
              <>
                <ul className="nav nav-tabs mb-4">
                  <li className="nav-item">
                    <button className="nav-link active">
                      Logowanie
                    </button>
                  </li>
                  <li className="nav-item">
                    <button
                      className="nav-link"
                      onClick={() => navigate('/register')}
                    >
                      Rejestracja
                    </button>
                  </li>
                </ul>
                <LoginForm 
                  onForgotPassword={() => navigate('/reset-password')}
                />
              </>
            } />
          </Routes>
        </div>
        <footer className="bg-light py-4 mt-5 border-top">
          <div className="container text-center text-muted">
            <p className="mb-0">
              SimpleBlog - Ręcznie Robione Ubrania &copy; 2024
            </p>
          </div>
        </footer>
      </div>
    );
  }

  return (
    <div className="min-vh-100 d-flex flex-column">
      <Header title="" subtitle="" />
      <div className="container-fluid flex-grow-1 d-flex flex-column">
        <Navigation onLogout={handleLogout} />
        <div className="flex-grow-1">
          <Routes>
            <Route path="/" element={renderHomePage()} />
            <Route path="/posts/:id" element={
              <PostDetailPage
                posts={posts}
                isAdmin={user?.role === 'Admin'}
                onDelete={deletePost}
                onEdit={(post) => {
                  setEditingPost(post);
                  setShowPostForm(true);
                }}
                onAddComment={addComment}
                onTogglePin={togglePin}
              />
            } />
            <Route path="/about" element={<AboutPage />} />
            <Route path="/shop" element={<ShopPage onViewCart={() => navigate('/cart')} />} />
            <Route path="/shop/:id" element={<ProductDetails />} />
            <Route path="/cart" element={<CartPage />} />
            <Route path="/checkout" element={<CheckoutPage />} />
            <Route path="/contact" element={<ContactPage />} />
            <Route path="/settings" element={
              user?.role === 'Admin' ? (
                <div className="container-fluid">
                  <div className="d-flex justify-content-between align-items-center mb-4">
                    <h2>
                      <i className="bi bi-gear-fill me-2" aria-hidden="true"></i>{' '}
                      Panel Administracyjny
                    </h2>
                    <button
                      className="btn btn-outline-secondary"
                      onClick={() => navigate('/')}
                    >
                      <i className="bi bi-arrow-left me-2" aria-hidden="true"></i>{' '}
                      Powrót
                    </button>
                  </div>
                  <AdminPanel showTitle={false} />
                </div>
              ) : (
                <Navigate to="/" replace />
              )
            } />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </div>
      </div>
      {showPostForm && (
        <PostForm
          post={editingPost}
          onSubmit={handlePostSubmit}
          onCancel={handlePostCancel}
          onAddImage={editingPost ? handleAddImage : undefined}
          onRemoveImage={editingPost ? handleRemoveImage : undefined}
        />
      )}
      <footer className="bg-light py-4 mt-5 border-top">
        <div className="container text-center text-muted">
          <p className="mb-0">
                  SimpleBlog - Ręcznie Robione Ubrania &copy; 2024
          </p>
        </div>
      </footer>
    </div>
  );
}

function App() {
  return (
    <BrowserRouter>
      <AppContent />
    </BrowserRouter>
  );
}

export default App;
