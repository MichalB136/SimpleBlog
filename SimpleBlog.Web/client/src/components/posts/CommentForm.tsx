import React, { useState } from 'react';
import type { CreateCommentRequest } from '@/types/post';

interface CommentFormProps {
  onAdd?: (payload: CreateCommentRequest) => void;
  disabled?: boolean;
}

export function CommentForm({ onAdd, disabled = false }: CommentFormProps) {
  const [author, setAuthor] = useState('');
  const [content, setContent] = useState('');
  const [error, setError] = useState('');

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (!content.trim()) {
      setError('Comment cannot be empty');
      return;
    }
    onAdd?.({ author: author || 'Anon', content });
    setAuthor('');
    setContent('');
  };

  return (
    <form className="row g-2 mb-3" onSubmit={submit}>
      <div className="col-md-3">
        <input
          className="form-control form-control-sm"
          placeholder="Twoje imię"
          value={author}
          onChange={(e) => setAuthor(e.target.value)}
          disabled={disabled}
        />
      </div>
      <div className="col-md-7">
        <input
          className="form-control form-control-sm"
          placeholder="Dodaj komentarz"
          value={content}
          onChange={(e) => setContent(e.target.value)}
          disabled={disabled}
          required
        />
      </div>
      <div className="col-md-2">
        <button className="btn btn-sm btn-outline-primary w-100" type="submit" disabled={disabled}>
          Wyślij
        </button>
      </div>
      {error && <div className="col-12"><small className="text-danger">{error}</small></div>}
    </form>
  );
}
