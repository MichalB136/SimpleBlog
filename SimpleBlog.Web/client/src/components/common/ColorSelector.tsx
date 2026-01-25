import { useState, KeyboardEvent } from 'react';

interface ColorSelectorProps {
  colors: string[];
  onChange: (colors: string[]) => void;
  disabled?: boolean;
}

export function ColorSelector({ colors, onChange, disabled }: ColorSelectorProps) {
  const [picker, setPicker] = useState('#6366f1');
  const [text, setText] = useState('');

  const addColor = (value?: string) => {
    const candidate = (value ?? text ?? picker).trim();
    if (!candidate) return;
    if (colors.includes(candidate)) {
      setText('');
      return;
    }
    onChange([...colors, candidate]);
    setText('');
  };

  const removeColor = (c: string) => {
    onChange(colors.filter(x => x !== c));
  };

  const onKey = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      addColor();
    }
  };

  return (
    <div>
      <div className="d-flex flex-wrap gap-2 mb-2">
        {colors.length === 0 && <small className="text-muted">Brak zdefiniowanych kolorów</small>}
        {colors.map(c => (
          <button
            key={c}
            type="button"
            className={`btn btn-sm ${disabled ? 'disabled' : 'btn-outline-secondary'}`}
            onClick={() => !disabled && removeColor(c)}
            title={c}
            style={{
              padding: 6,
              display: 'inline-flex',
              alignItems: 'center',
              gap: 8,
              borderColor: '#ddd',
              background: 'transparent'
            }}
          >
            <span style={{ width: 22, height: 22, display: 'inline-block', borderRadius: 6, background: c, border: '1px solid rgba(0,0,0,0.12)' }} />
            <small style={{ color: 'rgba(0,0,0,0.9)' }}>{c}</small>
          </button>
        ))}
      </div>

      <div className="d-flex gap-2">
        <input
          type="color"
          className="form-control form-control-color"
          value={picker}
          onChange={e => { setPicker(e.target.value); setText(e.target.value); }}
          disabled={disabled}
          title="Wybierz kolor"
          style={{ width: 56, padding: 0 }}
        />

        <input
          type="text"
          className="form-control form-control-sm"
          placeholder="#rrggbb or color name"
          value={text}
          onChange={e => setText(e.target.value)}
          onKeyDown={onKey}
          disabled={disabled}
        />

        <button type="button" className="btn btn-sm btn-outline-primary" onClick={() => addColor()} disabled={disabled || !text.trim()}>
          Dodaj
        </button>
      </div>
    </div>
  );
}

export default ColorSelector;
