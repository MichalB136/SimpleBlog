import { useState } from 'react';
import type { FormEvent } from 'react';
import { authApi } from '@/api/auth';
import type { ConfirmEmailRequest } from '@/types/auth';

interface ConfirmEmailFormProps {
  onSuccess?: () => void;
  onCancel?: () => void;
}

export function ConfirmEmailForm({ onSuccess, onCancel }: ConfirmEmailFormProps) {
  const [userId, setUserId] = useState('');
  const [token, setToken] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (!userId || !token) {
      setError('Wypełnij wszystkie pola');
      return;
    }

    setLoading(true);

    try {
      await authApi.confirmEmail({
        userId,
        token,
      } as ConfirmEmailRequest);
      
      setSuccess('E-mail został potwierdzon! Możesz się teraz zalogować.');
      setTimeout(() => {
        onSuccess?.();
      }, 2000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Błąd podczas potwierdzania e-mail');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="row justify-content-center mt-5">
      <div className="col-md-6 col-lg-4">
        <div className="card shadow">
          <div className="card-body">
            <h2 className="card-title text-center mb-4">Potwierdź e-mail</h2>
            <form onSubmit={handleSubmit}>
              <p className="text-muted text-center mb-3">
                Aby dokończyć rejestrację, potwierdź swój e-mail używając danych z wiadomości, którą otrzymałeś.
              </p>
              <div className="mb-3">
                <label className="form-label">ID użytkownika</label>
                <input
                  type="text"
                  className="form-control"
                  placeholder="Wklej ID z e-maila"
                  value={userId}
                  onChange={(e) => setUserId(e.target.value)}
                  required
                />
                <small className="text-muted d-block mt-1">
                  Znaldziesz to w linku potwierdzenia otrzymanym e-mailem
                </small>
              </div>
              <div className="mb-3">
                <label className="form-label">Token potwierdzenia</label>
                <input
                  type="text"
                  className="form-control"
                  placeholder="Wklej token z e-maila"
                  value={token}
                  onChange={(e) => setToken(e.target.value)}
                  required
                />
                <small className="text-muted d-block mt-1">
                  Znaldziesz to w linku potwierdzenia otrzymanym e-mailem
                </small>
              </div>
              <button
                type="submit"
                className="btn btn-primary w-100"
                disabled={loading}
              >
                {loading ? 'Potwierdzanie...' : 'Potwierdź e-mail'}
              </button>
              <button
                type="button"
                className="btn btn-outline-secondary w-100 mt-2"
                onClick={onCancel}
              >
                Anuluj
              </button>
              {error && (
                <div className="alert alert-danger mt-3 mb-0">{error}</div>
              )}
              {success && (
                <div className="alert alert-success mt-3 mb-0">{success}</div>
              )}
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}
