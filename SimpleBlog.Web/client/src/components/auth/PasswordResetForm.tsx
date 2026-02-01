import { useState } from 'react';
import type { FormEvent } from 'react';
import { authApi } from '@/api/auth';
import type { RequestPasswordResetRequest } from '@/types/auth';

interface PasswordResetFormProps {
  onSuccess?: () => void;
  onCancel?: () => void;
}

interface ResetStep {
  type: 'request' | 'reset';
  email?: string;
}

export function PasswordResetForm({ onSuccess, onCancel }: PasswordResetFormProps) {
  const [step, setStep] = useState<ResetStep>({ type: 'request' });
  const [email, setEmail] = useState('');
  const [userId, setUserId] = useState('');
  const [token, setToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const handleRequestReset = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (!email) {
      setError('Podaj adres e-mail');
      return;
    }

    setLoading(true);

    try {
      await authApi.requestPasswordReset({ email } as RequestPasswordResetRequest);
      setSuccess('Jeśli konto istnieje, otrzymasz e-mail z instrukcjami resetu hasła.');
      setEmail('');
      // Move to reset step
      setTimeout(() => {
        setStep({ type: 'reset', email });
      }, 2000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Błąd podczas wysyłania żądania');
    } finally {
      setLoading(false);
    }
  };

  const handleResetPassword = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (!userId || !token || !newPassword) {
      setError('Wypełnij wszystkie pola');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Hasła się nie zgadzają');
      return;
    }

    if (newPassword.length < 8) {
      setError('Hasło musi mieć co najmniej 8 znaków');
      return;
    }

    setLoading(true);

    try {
      await authApi.resetPassword({
        userId,
        token,
        newPassword,
      });
      setSuccess('Hasło zostało zmienione! Możesz się teraz zalogować.');
      setTimeout(() => {
        onSuccess?.();
      }, 2000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Błąd podczas resetowania hasła');
    } finally {
      setLoading(false);
    }
  };

  const handleBackToRequest = () => {
    setStep({ type: 'request' });
    setToken('');
    setUserId('');
    setNewPassword('');
    setConfirmPassword('');
    setSuccess('');
    setError('');
  };

  return (
    <div className="row justify-content-center mt-5">
      <div className="col-md-6 col-lg-4">
        <div className="card shadow">
          <div className="card-body">
            {step.type === 'request' ? (
              <>
                <h2 className="card-title text-center mb-4">Resetuj hasło</h2>
                <form onSubmit={handleRequestReset}>
                  <p className="text-muted text-center mb-3">
                    Podaj adres e-mail, na który zarejestrowane jest Twoje konto.
                  </p>
                  <div className="mb-3">
                    <label className="form-label">Adres e-mail</label>
                    <input
                      type="email"
                      className="form-control"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      required
                    />
                  </div>
                  <button
                    type="submit"
                    className="btn btn-primary w-100"
                    disabled={loading}
                  >
                    {loading ? 'Wysyłanie...' : 'Wyślij link resetu'}
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
              </>
            ) : (
              <>
                <h2 className="card-title text-center mb-4">Zmień hasło</h2>
                <form onSubmit={handleResetPassword}>
                  <p className="text-muted text-center mb-3">
                    Podaj ID użytkownika i token otrzymany e-mailem.
                  </p>
                  <div className="mb-3">
                    <label className="form-label">ID użytkownika</label>
                    <input
                      type="text"
                      className="form-control"
                      value={userId}
                      onChange={(e) => setUserId(e.target.value)}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Token z e-maila</label>
                    <input
                      type="text"
                      className="form-control"
                      value={token}
                      onChange={(e) => setToken(e.target.value)}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Nowe hasło</label>
                    <input
                      type="password"
                      className="form-control"
                      value={newPassword}
                      onChange={(e) => setNewPassword(e.target.value)}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Powtórz nowe hasło</label>
                    <input
                      type="password"
                      className="form-control"
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      required
                    />
                  </div>
                  <button
                    type="submit"
                    className="btn btn-primary w-100"
                    disabled={loading}
                  >
                    {loading ? 'Zmiana hasła...' : 'Zmień hasło'}
                  </button>
                  <button
                    type="button"
                    className="btn btn-outline-secondary w-100 mt-2"
                    onClick={handleBackToRequest}
                  >
                    Powrót
                  </button>
                  {error && (
                    <div className="alert alert-danger mt-3 mb-0">{error}</div>
                  )}
                  {success && (
                    <div className="alert alert-success mt-3 mb-0">{success}</div>
                  )}
                </form>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
