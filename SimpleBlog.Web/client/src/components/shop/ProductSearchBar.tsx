import { useState } from 'react';
import { useTags } from '@/hooks/useTags';

interface ProductSearchBarProps {
  onSearch: (filter: { tagIds?: string[], category?: string, searchTerm?: string }) => void;
  categories: string[];
}

export function ProductSearchBar({ onSearch, categories }: ProductSearchBarProps) {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('');
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const { tags } = useTags();

  const handleSearch = () => {
    onSearch({
      searchTerm: searchTerm.trim() || undefined,
      category: selectedCategory || undefined,
      tagIds: selectedTags.length > 0 ? selectedTags : undefined
    });
  };

  const handleClear = () => {
    setSearchTerm('');
    setSelectedCategory('');
    setSelectedTags([]);
    onSearch({});
  };

  const toggleTag = (tagId: string) => {
    setSelectedTags(prev => 
      prev.includes(tagId) 
        ? prev.filter(id => id !== tagId) 
        : [...prev, tagId]
    );
  };

  return (
    <div className="card mb-4">
      <div className="card-body">
        <h5 className="card-title mb-3">
          <i className="bi bi-funnel me-2"></i>
          Filtruj produkty
        </h5>
        
        {/* Search Input */}
        <div className="mb-3">
          <div className="input-group">
            <span className="input-group-text">
              <i className="bi bi-search"></i>
            </span>
            <input
              type="text"
              className="form-control"
              placeholder="Szukaj po nazwie lub opisie..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            />
          </div>
        </div>

        {/* Category Filter */}
        {categories.length > 0 && (
          <div className="mb-3">
            <label className="form-label small text-muted mb-2">Kategoria:</label>
            <select
              className="form-select"
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
            >
              <option value="">Wszystkie kategorie</option>
              {categories.map(cat => (
                <option key={cat} value={cat}>{cat}</option>
              ))}
            </select>
          </div>
        )}

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
            <i className="bi bi-funnel me-1"></i>
            Filtruj
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
        {(searchTerm || selectedCategory || selectedTags.length > 0) && (
          <div className="mt-3 pt-3 border-top">
            <small className="text-muted">
              <i className="bi bi-funnel me-1"></i>
              Aktywne filtry:
              {searchTerm && <span className="ms-2 badge bg-info">"{searchTerm}"</span>}
              {selectedCategory && <span className="ms-2 badge bg-secondary">{selectedCategory}</span>}
              {selectedTags.length > 0 && (
                <span className="ms-2 badge bg-primary">{selectedTags.length} {selectedTags.length === 1 ? 'tag' : 'tagów'}</span>
              )}
            </small>
          </div>
        )}
      </div>
    </div>
  );
}
