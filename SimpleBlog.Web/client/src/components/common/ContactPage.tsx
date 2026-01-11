import { useState } from 'react';

export function ContactPage() {
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    subject: '',
    message: ''
  });
  const [submitted, setSubmitted] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      // Simulate sending email (in production, call email service endpoint)
      console.log('Kontakt:', formData);
      
      // Reset form
      setFormData({ name: '', email: '', subject: '', message: '' });
      setSubmitted(true);
      
      // Hide success message after 5 seconds
      setTimeout(() => setSubmitted(false), 5000);
    } catch (err) {
      console.error('Błąd wysyłania:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="card shadow-sm">
      <div className="card-body p-5">
        <h2 className="mb-4">Kontakt</h2>
        
        {submitted && (
          <div className="alert alert-success alert-dismissible fade show" role="alert">
            <i className="bi bi-check-circle me-2"></i>
            <strong>Dziękujemy!</strong> Twoja wiadomość została wysłana. Odpowiemy jak najszybciej.
            <button 
              type="button" 
              className="btn-close" 
              onClick={() => setSubmitted(false)}
              aria-label="Close"
            ></button>
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label htmlFor="name" className="form-label">
              <i className="bi bi-person me-2"></i>
              Imię i nazwisko
            </label>
            <input
              type="text"
              className="form-control"
              id="name"
              name="name"
              value={formData.name}
              onChange={handleChange}
              required
              disabled={loading}
            />
          </div>

          <div className="mb-3">
            <label htmlFor="email" className="form-label">
              <i className="bi bi-envelope me-2"></i>
              Adres email
            </label>
            <input
              type="email"
              className="form-control"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
              disabled={loading}
            />
          </div>

          <div className="mb-3">
            <label htmlFor="subject" className="form-label">
              <i className="bi bi-chat-dots me-2"></i>
              Temat
            </label>
            <input
              type="text"
              className="form-control"
              id="subject"
              name="subject"
              value={formData.subject}
              onChange={handleChange}
              required
              disabled={loading}
            />
          </div>

          <div className="mb-3">
            <label htmlFor="message" className="form-label">
              <i className="bi bi-textarea me-2"></i>
              Wiadomość
            </label>
            <textarea
              className="form-control"
              id="message"
              name="message"
              rows={5}
              value={formData.message}
              onChange={handleChange}
              required
              disabled={loading}
            ></textarea>
          </div>

          <div className="d-grid gap-2">
            <button 
              type="submit" 
              className="btn btn-primary"
              disabled={loading}
            >
              {loading ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                  Wysyłanie...
                </>
              ) : (
                <>
                  <i className="bi bi-send me-2"></i>
                  Wyślij wiadomość
                </>
              )}
            </button>
          </div>
        </form>

        <div className="mt-5 pt-4 border-top">
          <h5 className="mb-4">Informacje kontaktowe</h5>
          <div className="row">
            <div className="col-md-6 mb-3">
              <h6 className="mb-2">
                <i className="bi bi-envelope text-primary me-2"></i>
                Email
              </h6>
              <p className="text-muted">contact@simpleblog.com</p>
            </div>
            <div className="col-md-6 mb-3">
              <h6 className="mb-2">
                <i className="bi bi-telephone text-primary me-2"></i>
                Telefon
              </h6>
              <p className="text-muted">+48 123 456 789</p>
            </div>
            <div className="col-md-6">
              <h6 className="mb-2">
                <i className="bi bi-geo-alt text-primary me-2"></i>
                Adres
              </h6>
              <p className="text-muted">ul. Example 123<br />00-000 Warszawa, Polska</p>
            </div>
            <div className="col-md-6">
              <h6 className="mb-2">
                <i className="bi bi-clock text-primary me-2"></i>
                Godziny otwarcia
              </h6>
              <p className="text-muted">Poniedziałek - Piątek: 9:00 - 18:00<br />Sobota - Niedziela: Zamknięte</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
