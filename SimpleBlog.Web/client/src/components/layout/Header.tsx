// React import not required with react-jsx

interface HeaderProps {
  title: string;
  subtitle: string;
}

export function Header({ title, subtitle }: HeaderProps) {
  return (
    <div className="text-center mb-4">
      <p className="text-primary text-uppercase fw-bold mb-2">SimpleBlog x Aspire</p>
      <h1 className="display-5 fw-bold mb-3">{title}</h1>
      <p className="text-muted">{subtitle}</p>
    </div>
  );
}
