import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App.tsx';
import { AuthProvider } from './context/AuthContext.tsx';
import './styles/globals.css';

// Apply initial theme from localStorage
const initialDark = localStorage.getItem('theme-mode') === 'dark';
document.documentElement.classList.toggle('dark-mode', initialDark);

ReactDOM.createRoot(document.getElementById('app')!).render(
  <React.StrictMode>
    <AuthProvider>
      <App />
    </AuthProvider>
  </React.StrictMode>
);
