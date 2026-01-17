# React Router Integration

> ## metadane dokumentu
> 
> ### ✅ wymagane
> **Tytuł:** React Router Integration  
> **Opis:** Przewodnik po implementacji React Router w SimpleBlog dla SPA routing  
> **Audience:** frontend developer  
> **Topic:** development  
> **Last Update:** 2026-01-17
>
> ### 📌 rekomendowane
> **Parent Document:** [README.md](../development/README.md)  
> **Difficulty:** intermediate  
> **Estimated Time:** 20 min  
> **Version:** 1.0.0  
> **Status:** approved
>
> ### 🏷️ opcjonalne
> **Prerequisites:** React, TypeScript, SPA concepts  
> **Related Docs:** [project-structure.md](../development/project-structure.md)  
> **Tags:** `react`, `routing`, `spa`, `navigation`, `typescript`

---

## 📋 przegląd

SimpleBlog używa React Router v6 dla obsługi routing client-side. Umożliwia to nawigację bez przeładowania strony oraz dedykowane URL-e dla każdej sekcji aplikacji.

---

## 🎯 struktura tras

```
/                    → Strona główna (lista postów)
/about              → O mnie
/shop               → Sklep
/cart               → Koszyk
/contact            → Kontakt
/settings           → Panel administracyjny (tylko dla adminów)
/login              → Logowanie (tylko dla niezalogowanych)
/register           → Rejestracja (tylko dla niezalogowanych)
```

---

## 💻 implementacja

### Instalacja Pakietów

```bash
npm install react-router-dom
npm install --save-dev @types/react-router-dom
```

### App.tsx Structure

```typescript
// SimpleBlog.Web/client/src/App.tsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
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
    // Routes for unauthenticated users
    return (
      <div className="min-vh-100 d-flex flex-column">
        <Header title="SimpleBlog × Aspire" subtitle="Prosty blog i sklep" />
        <div className="container my-4">
          <Routes>
            <Route path="/register" element={
              <>
                <ul className="nav nav-tabs mb-4">
                  <li className="nav-item">
                    <button className="nav-link" onClick={() => navigate('/login')}>
                      Logowanie
                    </button>
                  </li>
                  <li className="nav-item">
                    <button className="nav-link active">Rejestracja</button>
                  </li>
                </ul>
                <RegisterForm onSuccess={() => navigate('/login')} />
              </>
            } />
            <Route path="*" element={
              <>
                <ul className="nav nav-tabs mb-4">
                  <li className="nav-item">
                    <button className="nav-link active">Logowanie</button>
                  </li>
                  <li className="nav-item">
                    <button className="nav-link" onClick={() => navigate('/register')}>
                      Rejestracja
                    </button>
                  </li>
                </ul>
                <LoginForm />
              </>
            } />
          </Routes>
        </div>
      </div>
    );
  }

  // Routes for authenticated users
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
```

---

## 🧭 navigation component

### Link-Based Navigation

```typescript
// SimpleBlog.Web/client/src/components/layout/Navigation.tsx
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { useCart } from '@/hooks/useCart';

interface NavigationProps {
  onLogout: () => void;
}

export function Navigation({ onLogout }: NavigationProps) {
  const { user, logout } = useAuth();
  const { itemCount } = useCart();
  const location = useLocation();

  const tabs = [
    { id: 'home', label: 'Home', icon: 'house-door', path: '/' },
    { id: 'about', label: 'O mnie', icon: 'person', path: '/about' },
    { id: 'shop', label: 'Sklep', icon: 'shop', path: '/shop' },
    { id: 'contact', label: 'Kontakt', icon: 'envelope', path: '/contact' },
  ];

  return (
    <>
      <div className="d-flex justify-content-between align-items-center p-3 bg-light rounded mb-4">
        <div>
          <span className="text-muted me-2">Zalogowany:</span>
          <strong>{user?.username} </strong>
          <span className="badge bg-primary">{user?.role}</span>
        </div>
        <div className="d-flex align-items-center gap-2">
          {itemCount > 0 && (
            <div className="position-relative">
              <i className="bi bi-cart3 fs-5"></i>
              <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
                {itemCount}
              </span>
            </div>
          )}
          {user?.role === 'Admin' && (
            <Link
              to="/settings"
              className="btn btn-outline-secondary btn-sm"
              title="Panel administracyjny"
            >
              <i className="bi bi-gear-fill"></i>
            </Link>
          )}
          <button
            className="btn btn-outline-secondary btn-sm"
            onClick={() => {
              logout();
              onLogout();
            }}
          >
            <i className="bi bi-box-arrow-right me-1"></i>Wyloguj
          </button>
        </div>
      </div>

      <ul className="nav nav-tabs mb-4">
        {tabs.map((tab) => (
          <li key={tab.id} className="nav-item position-relative">
            <Link
              to={tab.path}
              className={`nav-link ${location.pathname === tab.path ? 'active' : ''}`}
            >
              <i className={`bi bi-${tab.icon} me-2`}></i>
              {tab.label}
              {tab.id === 'shop' && itemCount > 0 && (
                <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger" style={{ fontSize: '0.7rem' }}>
                  {itemCount}
                </span>
              )}
            </Link>
          </li>
        ))}
      </ul>
    </>
  );
}
```

---

## 🔐 protected routes

### Admin-Only Routes

Route `/settings` jest chroniona i wymaga roli Administrator:

```typescript
<Route path="/settings" element={
  user?.role === 'Admin' ? (
    <div className="container-fluid">
      <AdminPanel />
    </div>
  ) : (
    <Navigate to="/" replace />
  )
} />
```

### Authenticated Routes

Cała aplikacja wymaga logowania - niezalogowani użytkownicy widzą tylko `/login` i `/register`:

```typescript
if (!user) {
  return (
    <Routes>
      <Route path="/register" element={<RegisterForm />} />
      <Route path="*" element={<LoginForm />} />
    </Routes>
  );
}
```

---

## 🎯 programmatic navigation

### useNavigate Hook

```typescript
import { useNavigate } from 'react-router-dom';

function MyComponent() {
  const navigate = useNavigate();

  const handleSuccess = () => {
    // Redirect after successful operation
    navigate('/');
  };

  const handleBack = () => {
    // Navigate back in history
    navigate(-1);
  };

  const handleLogout = () => {
    // Navigate and replace current entry
    navigate('/', { replace: true });
  };

  return (
    <button onClick={handleSuccess}>Complete</button>
  );
}
```

### Navigation After Form Submit

```typescript
// RegisterForm component
export function RegisterForm({ onSuccess }: RegisterFormProps) {
  const navigate = useNavigate();

  const handleSubmit = async (data: RegisterFormData) => {
    await registerUser(data);
    // Programmatically navigate after successful registration
    navigate('/login');
  };

  return <form onSubmit={handleSubmit}>...</form>;
}
```

---

## 🌐 current location

### useLocation Hook

```typescript
import { useLocation } from 'react-router-dom';

function Navigation() {
  const location = useLocation();

  return (
    <ul className="nav nav-tabs">
      {tabs.map((tab) => (
        <li key={tab.id}>
          <Link
            to={tab.path}
            className={`nav-link ${location.pathname === tab.path ? 'active' : ''}`}
          >
            {tab.label}
          </Link>
        </li>
      ))}
    </ul>
  );
}
```

---

## 📝 url parameters

### Dynamic Routes (Future)

```typescript
// Example for post detail page
<Route path="/posts/:id" element={<PostDetail />} />

// In PostDetail component
import { useParams } from 'react-router-dom';

function PostDetail() {
  const { id } = useParams<{ id: string }>();
  
  // Fetch post by id
  const { post } = usePost(id);
  
  return <div>{post?.title}</div>;
}
```

### Query Parameters

```typescript
import { useSearchParams } from 'react-router-dom';

function SearchPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  
  const query = searchParams.get('q') || '';
  const category = searchParams.get('category') || 'all';

  const handleSearch = (newQuery: string) => {
    setSearchParams({ q: newQuery, category });
  };

  return (
    <div>
      <input 
        value={query} 
        onChange={(e) => handleSearch(e.target.value)} 
      />
    </div>
  );
}
```

---

## 🔄 lazy loading

### Code Splitting dla Większych Komponentów

```typescript
import { lazy, Suspense } from 'react';

const AdminPanel = lazy(() => import('@/components/admin/AdminPanel'));
const ShopPage = lazy(() => import('@/components/shop/ShopPage'));

function App() {
  return (
    <Routes>
      <Route path="/settings" element={
        <Suspense fallback={<div>Loading...</div>}>
          <AdminPanel />
        </Suspense>
      } />
      <Route path="/shop" element={
        <Suspense fallback={<div>Loading...</div>}>
          <ShopPage />
        </Suspense>
      } />
    </Routes>
  );
}
```

---

## 🎨 link styling

### Active Link Styling

```typescript
<Link
  to={tab.path}
  className={`nav-link ${location.pathname === tab.path ? 'active' : ''}`}
>
  {tab.label}
</Link>
```

### NavLink Component (Alternative)

```typescript
import { NavLink } from 'react-router-dom';

<NavLink
  to={tab.path}
  className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
>
  {tab.label}
</NavLink>
```

---

## 🔧 backend proxy configuration

### ASP.NET Core SPA Fallback

```csharp
// SimpleBlog.Web/Program.cs
if (!app.Environment.IsDevelopment())
{
    // Production: serve built SPA
    app.MapFallbackToFile("dist/index.html");
}
```

Wszystkie nieznane trasy są przekierowywane do `index.html`, gdzie React Router obsługuje routing client-side.

---

## 📊 routing patterns

### Nested Routes (Future Enhancement)

```typescript
<Route path="/shop" element={<ShopLayout />}>
  <Route index element={<ProductList />} />
  <Route path="products/:id" element={<ProductDetail />} />
  <Route path="categories/:category" element={<CategoryProducts />} />
  <Route path="*" element={<NotFound />} />
</Route>
```

### Layout Routes

```typescript
<Route element={<AuthenticatedLayout />}>
  <Route path="/" element={<Home />} />
  <Route path="/about" element={<About />} />
  <Route path="/settings" element={<Settings />} />
</Route>
```

---

## 🎯 best practices

### ✅ Zalecane

1. **Używaj `<Link>` zamiast `<a>`** - zapobiega przeładowaniu strony
2. **Centralizuj definicje tras** - łatwiejsza konserwacja
3. **Używaj lazy loading** - dla dużych komponentów
4. **Waliduj uprawnienia** - chronione trasy dla adminów
5. **Obsługuj 404** - catch-all route z redirect lub NotFound page
6. **Używaj `useNavigate` zamiast `window.location`** - zachowuje stan SPA
7. **Test routing logic** - upewnij się że redirecty działają poprawnie

### ❌ Unikaj

1. **Hardkodowania URL-i** - używaj stałych lub centralizacji
2. **`window.location.href`** - niszczy stan SPA
3. **Ignorowania SEO** - rozważ Server-Side Rendering w przyszłości
4. **Zbyt głębokich zagnieżdżeń** - utrudnia nawigację
5. **Braku breadcrumbs** - dla złożonych hierarchii
6. **Mieszania logiki routingu** - trzymaj w App.tsx lub dedykowanym pliku

---

## 🆘 troubleshooting

### Problem: Strona nie ładuje się po odświeżeniu (404)

**Przyczyna:** Backend nie zwraca `index.html` dla nieznanych tras

**Rozwiązanie:**
```csharp
// Dodaj SPA fallback
app.MapFallbackToFile("dist/index.html");
```

### Problem: Link nie aktywuje się poprawnie

**Przyczyna:** Nieprawidłowe porównanie ścieżek

**Rozwiązanie:**
```typescript
// Użyj exact match
className={`nav-link ${location.pathname === tab.path ? 'active' : ''}`}

// Lub NavLink
<NavLink 
  to={tab.path} 
  className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
  end  // Add for exact match
/>
```

### Problem: Navigate nie działa

**Przyczyna:** Wywołanie poza kontekstem Router

**Rozwiązanie:**
```typescript
// Upewnij się że komponent jest wewnątrz <BrowserRouter>
function App() {
  return (
    <BrowserRouter>
      <AppContent />  {/* useNavigate działa tutaj */}
    </BrowserRouter>
  );
}
```

---

## 📚 dodatkowe zasoby

- [React Router Documentation](https://reactrouter.com/en/main)
- [React Router Tutorial](https://reactrouter.com/en/main/start/tutorial)
- [SPA Best Practices](https://developer.mozilla.org/en-US/docs/Glossary/SPA)
- [Client-Side Routing](https://web.dev/rendering-on-the-web/)

---

## 🔄 plan rozwoju

### Przyszłe usprawnienia

1. **Dynamic routes** - `/posts/:id`, `/products/:id`
2. **Nested layouts** - Reusable layout components
3. **Route transitions** - Animacje między stronami
4. **Breadcrumbs** - Nawigacja hierarchiczna
5. **Route guards** - Middleware dla autoryzacji
6. **SEO optimization** - Server-Side Rendering lub Pre-rendering
