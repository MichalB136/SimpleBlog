import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App.tsx';
import { AuthProvider } from './context/AuthContext.tsx';
import './styles/globals.css';
import './styles/themes.css';

// Apply initial theme from localStorage
const savedTheme = localStorage.getItem('site-theme') || 'light';
document.documentElement.className = `theme-${savedTheme}`;

ReactDOM.createRoot(document.getElementById('app')!).render(
  <React.StrictMode>
    <AuthProvider>
      <App />
    </AuthProvider>
  </React.StrictMode>
);
