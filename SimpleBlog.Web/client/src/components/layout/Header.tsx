import { useSiteSettings } from '@/hooks/useSiteSettings';
import { buildResponsiveProps } from '@/utils/image';

interface HeaderProps {
  readonly title: string;
  readonly subtitle: string;
}

export function Header({ title, subtitle }: Readonly<HeaderProps>) {
  const { settings } = useSiteSettings();

  return (
    <div className="text-center mb-4">
      {settings?.logoUrl && (
        <div className="mb-3">
          {/* responsive logo */}
          <LogoImage src={`${settings.logoUrl}?t=${settings.updatedAt}`} />
        </div>
      )}
      <p className="text-primary text-uppercase fw-bold mb-2">SimpleBlog</p>
      <h1 className="display-5 fw-bold mb-3">{title}</h1>
      <p className="text-muted">{subtitle}</p>
    </div>
  );
}

function LogoImage({ src }: { src: string }) {
  const { src: s, srcSet, sizes } = buildResponsiveProps(src);
  return <img src={s} srcSet={srcSet} sizes={sizes} alt="Logo" style={{ maxHeight: '120px', maxWidth: '400px', objectFit: 'contain' }} className="img-fluid" />;
}
