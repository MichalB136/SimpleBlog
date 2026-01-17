import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Header } from '@/components/layout/Header';
import { Navigation } from '@/components/layout/Navigation';
import { usePosts } from '@/hooks/usePosts';
import { PostList } from '@/components/posts/PostList';
import { AboutPage } from '@/components/common/AboutPage';
import { ShopPage } from '@/components/shop/ShopPage';
import { CartPage } from '@/components/shop/CartPage';
import { ContactPage } from '@/components/common/ContactPage';
import { LoginForm } from '@/components/auth/LoginForm';
import { RegisterForm } from '@/components/auth/RegisterForm';
import { AdminPanel } from '@/components/admin/AdminPanel';
import { useAuth } from '@/context/AuthContext';
import { useNavigate } from 'react-router-dom';

function AppContent() {
  const { user } = useAuth();
  const { posts, loading, error, delete: deletePost, addComment, togglePin } = usePosts();
  const navigate = useNavigate();

  const handleLogout = () => {
    navigate('/');
  };

  if (!user) {
    return (
      <div className="min-vh-100 d-flex flex-column">
        <Header title="SimpleBlog × Aspire" subtitle="Prosty blog i sklep" />
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
                <LoginForm />
              </>
            } />
          </Routes>
        </div>
        <footer className="bg-light py-4 mt-5 border-top">
          <div className="container text-center text-muted">
            <p className="mb-0">
              SimpleBlog × Aspire &copy; 2024 | Powered by ASP.NET 9 & Vite React
            </p>
          </div>
        </footer>
      </div>
    );
  }

  return (
    <div className="min-vh-100 d-flex flex-column">
      <Header title="SimpleBlog × Aspire" subtitle="Prosty blog i sklep" />
      <div className="container-fluid flex-grow-1 d-flex flex-column">
        <Navigation onLogout={handleLogout} />
        <div className="flex-grow-1">
          <Routes>
            <Route path="/" element={
              loading ? (
                <p className="text-muted">Ładowanie postów...</p>
              ) : error ? (
                <div className="alert alert-danger">{error}</div>
              ) : (
                <PostList
                  posts={posts}
                  isAdmin={user?.role === 'Admin'}
                  onDelete={deletePost}
                  onAddComment={addComment}
                  onTogglePin={togglePin}
                />
              )
            } />
            <Route path="/about" element={<AboutPage />} />
            <Route path="/shop" element={<ShopPage onViewCart={() => navigate('/cart')} />} />
            <Route path="/cart" element={<CartPage />} />
            <Route path="/contact" element={<ContactPage />} />
            <Route path="/settings" element={
              user?.role === 'Admin' ? (
                <div className="container-fluid">
                  <div className="d-flex justify-content-between align-items-center mb-4">
                    <h2>
                      <i className="bi bi-gear-fill me-2"></i>
                      Panel Administracyjny
                    </h2>
                    <button
                      className="btn btn-outline-secondary"
                      onClick={() => navigate('/')}
                    >
                      <i className="bi bi-arrow-left me-2"></i>
                      Powrót
                    </button>
                  </div>
                  <AdminPanel />
                </div>
              ) : (
                <Navigate to="/" replace />
              )
            } />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </div>
      </div>
      <footer className="bg-light py-4 mt-5 border-top">
        <div className="container text-center text-muted">
          <p className="mb-0">
            SimpleBlog × Aspire &copy; 2024 | Powered by ASP.NET 9 & Vite React
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
