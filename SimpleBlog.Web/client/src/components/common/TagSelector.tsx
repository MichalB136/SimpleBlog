import { useState, useEffect } from 'react';
import { tagsApi, type Tag } from '../../api/tags';

interface TagSelectorProps {
  selectedTagIds: string[];
  onChange: (tagIds: string[]) => void;
  disabled?: boolean;
}

export function TagSelector({ selectedTagIds, onChange, disabled }: TagSelectorProps) {
  const [availableTags, setAvailableTags] = useState<Tag[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadTags();
  }, []);

  const loadTags = async () => {
    try {
      setLoading(true);
      const tags = await tagsApi.getAll();
      setAvailableTags(tags);
      setError(null);
    } catch (err) {
      setError('Nie udało się załadować tagów');
      console.error('Error loading tags:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleToggleTag = (tagId: string) => {
    if (disabled) return;

    const updated = selectedTagIds.includes(tagId)
      ? selectedTagIds.filter(id => id !== tagId)
      : [...selectedTagIds, tagId];
    
    onChange(updated);
  };

  if (loading) {
    return (
      <div className="text-muted">
        <small>Ładowanie tagów...</small>
      </div>
    );
  }

  if (error) {
    return (
      <div className="alert alert-warning alert-sm">
        {error}
      </div>
    );
  }

  if (availableTags.length === 0) {
    return (
      <div className="alert alert-info alert-sm">
        Brak dostępnych tagów. Utwórz tagi w panelu admina.
      </div>
    );
  }

  return (
    <div className="tag-selector">
      <div className="d-flex flex-wrap gap-2">
        {availableTags.map(tag => {
          const isSelected = selectedTagIds.includes(tag.id);
          return (
            <button
              key={tag.id}
              type="button"
              className={`btn btn-sm ${isSelected ? 'btn-primary' : 'btn-outline-secondary'}`}
              onClick={() => handleToggleTag(tag.id)}
              disabled={disabled}
              style={
                isSelected && tag.color
                  ? {
                      backgroundColor: tag.color,
                      borderColor: tag.color
                    }
                  : undefined
              }
            >
              {isSelected && <i className="bi bi-check-circle me-1"></i>}
              {tag.name}
            </button>
          );
        })}
      </div>
    </div>
  );
}

interface TagBadgesProps {
  tags: Tag[];
  onClick?: (tag: Tag) => void;
}

export function TagBadges({ tags, onClick }: TagBadgesProps) {
  if (tags.length === 0) {
    return null;
  }

  return (
    <div className="d-flex flex-wrap gap-1">
      {tags.map(tag => (
        <span
          key={tag.id}
          className={`badge ${onClick ? 'badge-clickable' : ''}`}
          style={{
            backgroundColor: tag.color || '#6c757d',
            cursor: onClick ? 'pointer' : 'default'
          }}
          onClick={() => onClick?.(tag)}
          role={onClick ? 'button' : undefined}
        >
          {tag.name}
        </span>
      ))}
    </div>
  );
}
