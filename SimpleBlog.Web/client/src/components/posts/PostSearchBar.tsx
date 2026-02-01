import { useState } from 'react';
import { useTags } from '@/hooks/useTags';

interface PostSearchBarProps {
  onSearch: (filter: { tagIds?: string[], searchTerm?: string }) => void;
}

export function PostSearchBar({ onSearch }: PostSearchBarProps) {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [isExpanded, setIsExpanded] = useState(false);
  const { tags } = useTags();

  const handleSearch = () => {
    onSearch({
      searchTerm: searchTerm.trim() || undefined,
      tagIds: selectedTags.length > 0 ? selectedTags : undefined
    });
  };

  const handleClear = () => {
    setSearchTerm('');
    setSelectedTags([]);
    onSearch({});
  };

  const toggleTag = (tagId: string) => {
    setSelectedTags(prev => {
      const updated = prev.includes(tagId) 
        ? prev.filter(id => id !== tagId) 
        : [...prev, tagId];
      
      // Apply filter immediately when tag is toggled
      onSearch({
        searchTerm: searchTerm.trim() || undefined,
        tagIds: updated.length > 0 ? updated : undefined
      });
      
      return updated;
    });
  };

  return (
    <div className="card mb-4">
      <div className="card-body">
        <div 
          className="d-flex justify-content-between align-items-center"
          style={{ cursor: 'pointer' }}
          onClick={() => setIsExpanded(!isExpanded)}
        >
          <h5 className="card-title mb-0">
            <i className="bi bi-search me-2"></i>
            Wyszukaj artykuły
          </h5>
          <i className={`bi bi-chevron-${isExpanded ? 'up' : 'down'}`}></i>
        </div>
        
        <div className={`collapse ${isExpanded ? 'show' : ''}`}>
          <div className="mt-3">
            {/* Search Input */}
            <div className="mb-3">
              <div className="input-group">
                <span className="input-group-text">
                  <i className="bi bi-search"></i>
                </span>
                <input
                  type="text"
                  className="form-control"
                  placeholder="Szukaj w tytule lub treści..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                />
              </div>
            </div>

            {/* Tag Filter */}
            {tags.length > 0 && (
              <div className="mb-3">
                <label className="form-label small text-muted mb-2">Filtruj po tagach:</label>
                <div className="d-flex flex-wrap gap-2">
                  {tags.map(tag => (
                    <button
                      key={tag.id}
                      className={`btn btn-sm ${selectedTags.includes(tag.id) ? 'btn-primary' : 'btn-outline-secondary'}`}
                      onClick={() => toggleTag(tag.id)}
                    >
                      {tag.name}
                    </button>
                  ))}
                </div>
              </div>
            )}

            {/* Action Buttons */}
            <div className="d-flex gap-2">
              <button 
                className="btn btn-primary"
                onClick={handleSearch}
              >
                <i className="bi bi-search me-1"></i>
                Szukaj
              </button>
              <button 
                className="btn btn-outline-secondary"
                onClick={handleClear}
              >
                <i className="bi bi-x-circle me-1"></i>
                Wyczyść
              </button>
            </div>

            {/* Active Filters Info */}
            {(searchTerm || selectedTags.length > 0) && (
              <div className="mt-3 pt-3 border-top">
                <small className="text-muted">
                  <i className="bi bi-funnel me-1"></i>
                  Aktywne filtry:
                  {searchTerm && <span className="ms-2 badge bg-info">"{searchTerm}"</span>}
                  {selectedTags.length > 0 && (
                    <span className="ms-2 badge bg-primary">{selectedTags.length} {selectedTags.length === 1 ? 'tag' : 'tagów'}</span>
                  )}
                </small>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
