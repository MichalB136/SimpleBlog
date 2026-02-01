interface HeaderProps {
  readonly title: string;
  readonly subtitle: string;
}

export function Header({ title, subtitle }: Readonly<HeaderProps>) {
  return (
    <div className="text-center mb-4">
      <h1 className="display-5 fw-bold mb-3">{title}</h1>
      <p className="text-muted">{subtitle}</p>
    </div>
  );
}
