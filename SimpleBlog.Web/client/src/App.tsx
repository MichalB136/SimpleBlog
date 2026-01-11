import { useState } from 'react';
import { Header } from '@/components/layout/Header';
import { Navigation } from '@/components/layout/Navigation';
import { ThemeToggle } from '@/components/layout/ThemeToggle';
import { usePosts } from '@/hooks/usePosts';
import { PostList } from '@/components/posts/PostList';
import { AboutPage } from '@/components/common/AboutPage';
import { ShopPage } from '@/components/shop/ShopPage';
import { ContactPage } from '@/components/common/ContactPage';
import { LoginForm } from '@/components/auth/LoginForm';
import { RegisterForm } from '@/components/auth/RegisterForm';
import { useAuth } from '@/context/AuthContext';

type Tab = 'home' | 'about' | 'shop' | 'contact' | 'login' | 'register';

function App() {
  const [activeTab, setActiveTab] = useState<Tab>('home');
  const { user } = useAuth();
  const { posts, loading, error, delete: deletePost, addComment, togglePin } = usePosts();
  const [isDark, setIsDark] = useState<boolean>(() => {
    return localStorage.getItem('theme-mode') === 'dark';
  });

  const handleToggleTheme = () => {
    const next = !isDark;
    setIsDark(next);
    localStorage.setItem('theme-mode', next ? 'dark' : 'light');
    const root = document.documentElement;
    root.classList.toggle('dark-mode', next);
  };

  const handleTabChange = (tab: Tab) => {
    setActiveTab(tab);
  };
  const handleNavTabChange = (tabId: string) => {
    setActiveTab(tabId as Tab);
  };

  const handleLogout = () => {
    setActiveTab('home');
  };

  return (
    <div className="min-vh-100 d-flex flex-column">
      <ThemeToggle isDark={isDark} onToggle={handleToggleTheme} />

      {!user ? (
        <>
          <Header title="SimpleBlog × Aspire" subtitle="Prosty blog i sklep" />
          <div className="container my-4">
            <ul className="nav nav-tabs mb-4">
              <li className="nav-item">
                <button
                  className={`nav-link ${activeTab === 'login' ? 'active' : ''}`}
                  onClick={() => handleTabChange('login')}
                >
                  Logowanie
                </button>
              </li>
              <li className="nav-item">
                <button
                  className={`nav-link ${activeTab === 'register' ? 'active' : ''}`}
                  onClick={() => handleTabChange('register')}
                >
                  Rejestracja
                </button>
              </li>
            </ul>
            {activeTab === 'login' && <LoginForm />}
            {activeTab === 'register' && <RegisterForm onSuccess={() => setActiveTab('login')} />}
          </div>
        </>
      ) : (
        <>
          <Header title="SimpleBlog × Aspire" subtitle="Prosty blog i sklep" />
          <div className="container-fluid flex-grow-1 d-flex flex-column">
            <Navigation
              activeTab={activeTab}
              onTabChange={handleNavTabChange}
              onLogout={handleLogout}
            />
            <div className="flex-grow-1">
              {activeTab === 'home' && (
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
              )}
              {activeTab === 'about' && <AboutPage />}
              {activeTab === 'shop' && <ShopPage />}
              {activeTab === 'contact' && <ContactPage />}
            </div>
          </div>
        </>
      )}

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

export default App;
