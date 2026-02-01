import { useState } from 'react';
import type { FormEvent } from 'react';
import { useAuth } from '@/context/AuthContext';
import type { RegisterRequest } from '@/types/auth';
import { ApiError } from '@/api/client';

interface RegisterFormProps {
  onSuccess?: () => void;
  onCancel?: () => void;
}

export function RegisterForm({ onSuccess, onCancel }: RegisterFormProps) {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [validationErrors, setValidationErrors] = useState<Record<string, string[]>>({});
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setValidationErrors({});

    if (password !== confirmPassword) {
      setError('Hasła się nie zgadzają');
      return;
    }

    setLoading(true);

    try {
      await register({ username, email, password } as RegisterRequest);
      onSuccess?.();
    } catch (err) {
      console.error('Register error:', err);
      
      // Check if it's an ApiError with validation errors
      if (err instanceof ApiError && err.validationErrors) {
        console.log('Setting validation errors from ApiError:', err.validationErrors);
        setValidationErrors(err.validationErrors);
        setError(err.message);
      } else {
        const errorMsg = err instanceof Error ? err.message : 'Registration failed';
        console.log('Raw error message:', errorMsg);
        
        // Try to parse error message if it's a JSON string
        try {
          if (errorMsg.startsWith('{')) {
            const parsedError = JSON.parse(errorMsg);
            console.log('Parsed error from JSON:', parsedError);
            
            if (parsedError.errors && typeof parsedError.errors === 'object') {
              console.log('Found validation errors in parsed JSON:', parsedError.errors);
              setValidationErrors(parsedError.errors);
              setError(parsedError.title || 'One or more validation errors occurred.');
            } else {
              setError(parsedError.title || errorMsg);
            }
          } else {
            setError(errorMsg);
          }
        } catch (parseErr) {
          console.log('Could not parse error as JSON, using as is');
          setError(errorMsg);
        }
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="row justify-content-center mt-5">
      <div className="col-md-6 col-lg-4">
        <div className="card shadow">
          <div className="card-body">
            <h2 className="card-title text-center mb-4">Zarejestruj się</h2>
            <form onSubmit={handleSubmit}>
              <div className="mb-3">
                <label className="form-label">Nazwa użytkownika</label>
                <input
                  type="text"
                  className="form-control"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  required
                />
              </div>
              <div className="mb-3">
                <label className="form-label">Email</label>
                <input
                  type="email"
                  className="form-control"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                />
              </div>
              <div className="mb-3">
                <label className="form-label">Hasło</label>
                <input
                  type="password"
                  className="form-control"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
              </div>
              <div className="mb-3">
                <label className="form-label">Powtórz hasło</label>
                <input
                  type="password"
                  className="form-control"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  required
                />
              </div>
              <button type="submit" className="btn btn-primary w-100" disabled={loading}>
                {loading ? 'Rejestrowanie...' : 'Utwórz konto'}
              </button>
              <button type="button" className="btn btn-outline-secondary w-100 mt-2" onClick={onCancel}>
                Wróć do logowania
              </button>
              {error && (
                <div className="alert alert-danger mt-3 mb-0">
                  <p className="mb-2 fw-bold">{error}</p>
                  {Object.entries(validationErrors).length > 0 && (
                    <div>
                      {Object.entries(validationErrors).map(([field, messages]) => (
                        <div key={field} className="mb-2">
                          <p className="mb-1 fw-semibold text-uppercase" style={{fontSize: '0.85rem'}}>
                            {field}
                          </p>
                          <ul className="mb-0 ps-3" style={{fontSize: '0.9rem'}}>
                            {messages.map((msg, idx) => (
                              <li key={idx} className="mb-1">{msg}</li>
                            ))}
                          </ul>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}
