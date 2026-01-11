# SimpleBlog Frontend - Vite + React + TypeScript Migration

## Overview

Frontend został przeniesiony z monolitycznej aplikacji React UMD do nowoczesnego stacku:
- **Vite 6.0.3** - Fast build tool
- **React 18.3.1** - UI library
- **TypeScript 5.6.3** - Type safety
- **Bootstrap 5.3.0** - Styling

## Project Structure

```
client/
├── public/              # Static assets
├── src/
│   ├── api/            # API service layer (client.ts + feature modules)
│   ├── components/     # React components (organized by feature)
│   ├── context/        # Context API (auth state management)
│   ├── hooks/          # Custom hooks (data fetching, local storage)
│   ├── styles/         # Global CSS with dark mode support
│   ├── types/          # TypeScript interfaces (API contracts)
│   ├── App.tsx         # Main app component with routing logic
│   └── main.tsx        # Entry point
├── index.html          # HTML template
├── vite.config.ts      # Vite configuration
├── tsconfig.json       # TypeScript configuration
└── package.json        # Dependencies
```

## Local Development

### Prerequisites
- Node.js 20+ (LTS recommended)
- npm or yarn

### Setup

```bash
# Navigate to client directory
cd SimpleBlog.Web/client

# Install dependencies
npm install

# Start development server
npm run dev

# The Vite dev server runs on http://localhost:5173
# API requests are proxied to http://localhost:5000/api
```

### Available Scripts

```bash
npm run dev      # Start development server with hot reload
npm run build    # Build for production (outputs to dist/)
npm run preview  # Preview production build locally
npm run type-check # Run TypeScript type checking
```

## API Integration

All API calls go through `src/api/client.ts`, which automatically:
1. Injects Bearer token from localStorage
2. Sets correct Content-Type headers
3. Handles error responses
4. Provides typed responses

### Making API Calls

```typescript
// Use service modules (automatically typed)
import { postsApi } from '@/api/posts';

const posts = await postsApi.getPosts();
await postsApi.pin(postId);
await postsApi.addComment(postId, { content: 'Great post!' });
```

## Authentication

Authentication context (`src/context/AuthContext.tsx`) provides:
- Global `user` state
- `login(username, password)` method
- `register(username, email, password)` method
- `logout()` method
- Token persistence to localStorage

Usage in components:
```typescript
import { useAuth } from '@/context/AuthContext';

export function MyComponent() {
  const { user, login, logout } = useAuth();
  // ...
}
```

## Dark Mode

Theme toggle is available in bottom-right corner (`ThemeToggle.tsx`).
Preference is saved to localStorage as `theme-mode`.

CSS variables automatically switch between light/dark modes:
- `--bs-body-bg`
- `--bs-body-color`
- `--bs-border-color`

## Building for Production

```bash
cd SimpleBlog.Web/client
npm run build

# Output: dist/ folder with optimized production build
# These files will be copied to wwwroot/dist/ by Dockerfile
```

## Docker Build

The multi-stage Dockerfile handles:
1. **Frontend build** (Node stage): Installs deps + runs `npm run build`
2. **Backend build** (.NET stage): Publishes .NET app
3. **Runtime** (ASP.NET): Serves Vite build from `wwwroot/dist/`

Build command:
```bash
docker build -t simpleblog-web:latest -f SimpleBlog.Web/Dockerfile .
```

## Deployment to Render

The `render.yaml` already includes:
- Build command: `npm run build` (in client/ context)
- Service definition for Web app

No changes needed - just push to GitHub and Render deploys automatically.

## TypeScript Configuration

- **Strict mode** enabled for maximum type safety
- **Path alias** `@/*` points to `src/`
- **JSX as react-jsx** for automatic React import

## Browser Support

- Chrome/Edge: Latest 2 versions
- Firefox: Latest 2 versions
- Safari: Latest 2 versions

## Troubleshooting

### Port conflicts
If port 5173 is already in use:
```bash
npm run dev -- --port 3000
```

### API not responding
Check that:
1. Backend is running on http://localhost:5000
2. `vite.config.ts` proxy is configured correctly
3. Token is in localStorage (check DevTools)

### Build fails
```bash
# Clear node_modules and reinstall
rm -rf node_modules package-lock.json
npm install
npm run build
```

### CORS issues
Vite proxy handles this in development. In production, CORS is configured in backend.

## Performance

Vite optimizations:
- Code splitting by route
- Lazy component loading via React.lazy
- Dynamic imports for heavy components
- Tree-shaking of unused code
- CSS minification

Expected bundle size: ~180KB (gzipped ~50KB)

## Key Improvements Over Old Frontend

| Aspect | Before | After |
|--------|--------|-------|
| Language | Plain JS + React.createElement | TypeScript + JSX |
| File Size | 2027 lines (1 file) | 2500 lines (30+ files) |
| Type Safety | 0% | 100% (strict mode) |
| Development | Manual HTML changes | Hot reload + auto-compile |
| Maintainability | Very hard (monolith) | Easy (modular) |
| IDE Support | Basic | Full IntelliSense |
| Build time | N/A | ~2s (dev), ~5s (prod) |

## Migration Notes

- Old `wwwroot/app.js` is preserved for reference
- New build goes to `wwwroot/dist/`
- Authentication token persistence identical
- API contracts unchanged
- Bootstrap classes preserved for styling

## Contributing

When adding new features:
1. Create a new component in `src/components/`
2. Add TypeScript types if needed in `src/types/`
3. Use custom hooks from `src/hooks/`
4. Call API via services in `src/api/`
5. Run `npm run build` before committing

---

For questions, see `docs/` folder for additional documentation.
