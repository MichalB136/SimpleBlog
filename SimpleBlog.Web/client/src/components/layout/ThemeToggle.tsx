interface ThemeToggleProps {
  isDark: boolean;
  onToggle: () => void;
}

export function ThemeToggle({ isDark, onToggle }: ThemeToggleProps) {
  return (
    <button
      className="theme-toggle"
      onClick={onToggle}
      title="Przełącz motyw"
      style={{
        position: 'fixed',
        bottom: '20px',
        right: '20px',
        backgroundColor: 'transparent',
        border: 'none',
        fontSize: '24px',
        cursor: 'pointer',
        zIndex: 1000,
      }}
    >
      <i className={isDark ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill'}></i>
    </button>
  );
}
