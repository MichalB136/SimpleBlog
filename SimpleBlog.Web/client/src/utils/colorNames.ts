import colorNamer from 'color-namer';

function normalizeHex(input: string): string {
  if (!input) return input;
  const s = input.trim().toLowerCase();
  if (s.startsWith('#')) {
    // Expand shorthand #abc -> #aabbcc
    if (s.length === 4) {
      return '#' + s[1] + s[1] + s[2] + s[2] + s[3] + s[3];
    }
    // If 7 or longer, take first 7 (#rrggbb)
    return s.length >= 7 ? s.slice(0, 7) : s;
  }
  return s;
}

// Note: color name resolution delegated to `color-namer`.

export function getColorName(code: string): string {
  if (!code) return 'Kolor';
  if (code.startsWith('http')) return 'Wzór';
  const hex = normalizeHex(code);
  if (!hex) return code;

  try {
    const names = colorNamer(hex);
    // Prefer ntc (nearest color name) then basic.
    const ntc = names?.ntc?.[0]?.name;
    if (ntc) return translateToPolish(ntc);
    const basic = names?.basic?.[0]?.name;
    if (basic) return translateToPolish(basic);
    // Fallback to first available group
    for (const group of Object.values(names || {})) {
      if (Array.isArray(group) && group.length > 0 && group[0].name)
        return translateToPolish(group[0].name);
    }
  } catch (e) {
    // ignore and fallback
  }

  return code;
}

export default getColorName;

function translateToPolish(eng: string): string {
  if (!eng) return eng;
  const tokenMap: Record<string, string> = {
    black: 'Czarny',
    white: 'Biały',
    red: 'Czerwony',
    green: 'Zielony',
    blue: 'Niebieski',
    yellow: 'Żółty',
    cyan: 'Cyjan',
    magenta: 'Magenta',
    gray: 'Szary',
    grey: 'Szary',
    orange: 'Pomarańczowy',
    purple: 'Fioletowy',
    pink: 'Różowy',
    brown: 'Brązowy',
    navy: 'Granatowy',
    teal: 'Morski',
    lime: 'Limonkowy',
    olive: 'Oliwkowy',
    maroon: 'Bordowy',
    gold: 'Złoty',
    silver: 'Srebrny',
    beige: 'Beżowy',
    transparent: 'Przezroczysty',
    // modifiers
    light: 'Jasny',
    dark: 'Ciemny',
    medium: 'Średni',
    pale: 'Blady',
    deep: 'Głęboki',
  };

  // Split by non-word characters to preserve phrases like "Light Sea Green"
  const parts = eng.split(/\s+/).map(p => p.trim()).filter(Boolean);
  const translated = parts.map(part => {
    const key = part.toLowerCase();
    // If exact match in map, use it
    if (tokenMap[key]) return tokenMap[key];
    // Try removing punctuation
    const cleaned = key.replace(/[^a-z]/g, '');
    if (tokenMap[cleaned]) return tokenMap[cleaned];
    // Capitalize unknown token's first letter
    return part.charAt(0).toUpperCase() + part.slice(1).toLowerCase();
  });

  return translated.join(' ');
}
