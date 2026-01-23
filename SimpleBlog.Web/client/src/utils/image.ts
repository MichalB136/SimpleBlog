export function appendWidth(url: string, width: number) {
  try {
    const u = new URL(url);
    // If Cloudinary URL (contains /upload/), inject transformation
    const uploadIndex = u.pathname.indexOf('/upload/');
    if (uploadIndex !== -1) {
      const before = u.origin + u.pathname.slice(0, uploadIndex + 8); // include '/upload/'
      const after = u.pathname.slice(uploadIndex + 8) + u.search + u.hash;
      return `${before}w_${width},c_scale/${after}`;
    }

    // Fallback: add or replace ?w= query parameter
    u.searchParams.set('w', String(width));
    return u.toString();
  } catch {
    // If not a valid URL, fallback to query param approach
    if (url.includes('?')) return `${url}&w=${width}`;
    return `${url}?w=${width}`;
  }
}

export function buildResponsiveProps(url: string | undefined | null) {
  if (!url) return { src: '', srcSet: '', sizes: '' };
  const widths = [360, 768, 1200];
  const srcSet = widths.map((w) => `${appendWidth(url, w)} ${w}w`).join(', ');
  const src = appendWidth(url, 800);
  const sizes = '(max-width: 576px) 100vw, (max-width: 992px) 50vw, 33vw';
  return { src, srcSet, sizes };
}
