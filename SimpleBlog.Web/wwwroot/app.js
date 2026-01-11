const { useEffect, useMemo, useState } = React;

const apiBase = "/api";

let authToken = localStorage.getItem("authToken") || null;
let currentUser = JSON.parse(localStorage.getItem("currentUser") || "null");

// Theme management
const getTheme = () => localStorage.getItem("theme") || "light";
const setTheme = (theme) => {
  localStorage.setItem("theme", theme);
  document.documentElement.setAttribute("data-theme", theme);
};

// Initialize theme on load
setTheme(getTheme());

async function request(path, options = {}) {
  const headers = {
    "Content-Type": "application/json",
    ...(authToken ? { "Authorization": `Bearer ${authToken}` } : {})
  };

  const response = await fetch(`${apiBase}${path}`, {
    headers,
    ...options,
  });

  const contentType = response.headers.get("content-type");
  const isJson = contentType && contentType.includes("application/json");
  const payload = isJson ? await response.json() : await response.text();

  if (!response.ok) {
    const detail = typeof payload === "string" ? payload : payload?.title || "Wystąpił błąd";
    throw new Error(detail);
  }

  return payload;
}

function LoginForm({ onLogin, onShowRegister }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const response = await request("/login", {
        method: "POST",
        body: JSON.stringify({ username, password }),
      });

      authToken = response.token;
      currentUser = { username: response.username, role: response.role };
      localStorage.setItem("authToken", authToken);
      localStorage.setItem("currentUser", JSON.stringify(currentUser));

      onLogin(currentUser);
    } catch (err) {
      setError("Nieprawidłowe dane logowania");
    } finally {
      setLoading(false);
    }
  };

  return React.createElement(
    "div",
    { className: "row justify-content-center mt-5" },
    React.createElement(
      "div",
      { className: "col-md-6 col-lg-4" },
      React.createElement(
        "div",
        { className: "card shadow" },
        React.createElement(
          "div",
          { className: "card-body" },
          React.createElement("h2", { className: "card-title text-center mb-4" }, "Zaloguj się"),
          React.createElement(
            "form",
            { onSubmit: handleSubmit },
            React.createElement(
              "div",
              { className: "mb-3" },
              React.createElement("label", { className: "form-label" }, "Nazwa użytkownika"),
              React.createElement("input", {
                type: "text",
                className: "form-control",
                value: username,
                onChange: (e) => setUsername(e.target.value),
                required: true,
              })
            ),
            React.createElement(
              "div",
              { className: "mb-3" },
              React.createElement("label", { className: "form-label" }, "Hasło"),
              React.createElement("input", {
                type: "password",
                className: "form-control",
                value: password,
                onChange: (e) => setPassword(e.target.value),
                required: true,
              })
            ),
            React.createElement(
              "button",
              { type: "submit", className: "btn btn-primary w-100", disabled: loading },
              loading ? "Logowanie..." : "Zaloguj"
            ),
            React.createElement(
              "button",
              { type: "button", className: "btn btn-outline-secondary w-100 mt-2", onClick: () => onShowRegister?.() },
              "Zarejestruj się"
            ),
            error ? React.createElement("div", { className: "alert alert-danger mt-3 mb-0" }, error) : null
          )
        )
      )
    )
  );
}

function RegisterForm({ onRegistered, onCancel }) {
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (password !== confirmPassword) {
      setError("Hasła muszą być takie same");
      return;
    }

    setLoading(true);
    try {
      await request("/register", {
        method: "POST",
        body: JSON.stringify({ username, email, password }),
      });
      onRegistered?.();
    } catch (err) {
      setError(err.message || "Rejestracja nie powiodła się");
    } finally {
      setLoading(false);
    }
  };

  return React.createElement(
    "div",
    { className: "row justify-content-center mt-5" },
    React.createElement(
      "div",
      { className: "col-md-6 col-lg-4" },
      React.createElement(
        "div",
        { className: "card shadow" },
        React.createElement(
          "div",
          { className: "card-body" },
          React.createElement("h2", { className: "card-title text-center mb-4" }, "Zarejestruj się"),
          React.createElement(
            "form",
            { onSubmit: handleSubmit },
            React.createElement(
              "div",
              { className: "mb-3" },
              React.createElement("label", { className: "form-label" }, "Nazwa użytkownika"),
              React.createElement("input", {
                type: "text",
                className: "form-control",
                value: username,
                onChange: (e) => setUsername(e.target.value),
                required: true,
              })
            ),
            React.createElement(
              "div",
              { className: "mb-3" },
              React.createElement("label", { className: "form-label" }, "Email"),
              React.createElement("input", {
                type: "email",
                className: "form-control",
                value: email,
                onChange: (e) => setEmail(e.target.value),
                required: true,
              })
            ),
            React.createElement(
              "div",
              { className: "mb-3" },
              React.createElement("label", { className: "form-label" }, "Hasło"),
              React.createElement("input", {
                type: "password",
                className: "form-control",
                value: password,
                onChange: (e) => setPassword(e.target.value),
                required: true,
              })
            ),
            React.createElement(
              "div",
              { className: "mb-3" },
              React.createElement("label", { className: "form-label" }, "Powtórz hasło"),
              React.createElement("input", {
                type: "password",
                className: "form-control",
                value: confirmPassword,
                onChange: (e) => setConfirmPassword(e.target.value),
                required: true,
              })
            ),
            React.createElement(
              "button",
              { type: "submit", className: "btn btn-primary w-100", disabled: loading },
              loading ? "Rejestrowanie..." : "Utwórz konto"
            ),
            React.createElement(
              "button",
              { type: "button", className: "btn btn-outline-secondary w-100 mt-2", onClick: () => onCancel?.() },
              "Wróć do logowania"
            ),
            error ? React.createElement("div", { className: "alert alert-danger mt-3 mb-0" }, error) : null
          )
        )
      )
    )
  );
}

function PostForm({ onCreated, isSubmitting }) {
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [author, setAuthor] = useState("");
  const [imageUrl, setImageUrl] = useState("");
  const [error, setError] = useState("");

  const handleImageChange = (e) => {
    const file = e.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (event) => {
        setImageUrl(event.target?.result || "");
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (!title.trim() || !content.trim()) {
      setError("Tytuł i treść są wymagane");
      return;
    }

    try {
      const payload = { title: title.trim(), content: content.trim(), author: author.trim(), imageUrl: imageUrl || null };
      await onCreated(payload);
      setTitle("");
      setContent("");
      setAuthor("");
      setImageUrl("");
    } catch (err) {
      setError(err.message);
    }
  };

  return React.createElement(
    "div",
    { className: "card shadow mb-4" },
    React.createElement(
      "div",
      { className: "card-header bg-primary text-white d-flex justify-content-between align-items-center" },
      React.createElement(
        "div",
        null,
        React.createElement("small", { className: "text-uppercase d-block mb-1" }, "Nowy wpis"),
        React.createElement("h5", { className: "mb-0" }, "Dodaj notkę")
      ),
      React.createElement(
        "button",
        { type: "submit", form: "post-form", className: "btn btn-light btn-sm", disabled: isSubmitting },
        isSubmitting ? "Zapisywanie..." : React.createElement("span", null, React.createElement("i", { className: "bi bi-send me-1" }), "Publikuj")
      )
    ),
    React.createElement(
      "div",
      { className: "card-body" },
      React.createElement(
        "form",
        { id: "post-form", onSubmit: handleSubmit },
        React.createElement(
          "div",
          { className: "mb-3" },
          React.createElement("label", { className: "form-label" }, "Tytuł"),
          React.createElement("input", {
            className: "form-control",
            value: title,
            onChange: (e) => setTitle(e.target.value),
            placeholder: "Mój pierwszy wpis",
            required: true,
          })
        ),
        React.createElement(
          "div",
          { className: "mb-3" },
          React.createElement("label", { className: "form-label" }, "Autor"),
          React.createElement("input", {
            className: "form-control",
            value: author,
            onChange: (e) => setAuthor(e.target.value),
            placeholder: "Twoje imię",
          })
        ),
        React.createElement(
          "div",
          { className: "mb-3" },
          React.createElement("label", { className: "form-label" }, "Treść"),
          React.createElement("textarea", {
            className: "form-control",
            rows: "5",
            value: content,
            onChange: (e) => setContent(e.target.value),
            placeholder: "Co masz dziś do powiedzenia?",
            required: true,
          })
        ),
        React.createElement(
          "div",
          { className: "mb-3" },
          React.createElement("label", { className: "form-label" }, "Zdjęcie (opcjonalnie)"),
          React.createElement("input", {
            type: "file",
            className: "form-control",
            accept: "image/*",
            onChange: handleImageChange,
          }),
          imageUrl && React.createElement("img", { src: imageUrl, className: "img-fluid rounded mt-2", style: { maxHeight: "200px" } })
        ),
        error ? React.createElement("div", { className: "alert alert-danger" }, error) : null
      )
    )
  );
}

function CommentForm({ onAdd, disabled }) {
  const [author, setAuthor] = useState("");
  const [content, setContent] = useState("");
  const [error, setError] = useState("");

  const submit = async (e) => {
    e.preventDefault();
    setError("");
    if (!content.trim()) {
      setError("Treść komentarza jest wymagana");
      return;
    }
    try {
      await onAdd({ author: author.trim(), content: content.trim() });
      setContent("");
      setAuthor("");
    } catch (err) {
      setError(err.message);
    }
  };

  return React.createElement(
    "form",
    { className: "row g-2 mb-3", onSubmit: submit },
    React.createElement(
      "div",
      { className: "col-md-3" },
      React.createElement("input", {
        className: "form-control form-control-sm",
        placeholder: "Twoje imię",
        value: author,
        onChange: (e) => setAuthor(e.target.value),
        disabled: disabled,
      })
    ),
    React.createElement(
      "div",
      { className: "col-md-7" },
      React.createElement("input", {
        className: "form-control form-control-sm",
        placeholder: "Dodaj komentarz",
        value: content,
        onChange: (e) => setContent(e.target.value),
        disabled: disabled,
        required: true,
      })
    ),
    React.createElement(
      "div",
      { className: "col-md-2" },
      React.createElement(
        "button",
        { className: "btn btn-sm btn-outline-primary w-100", type: "submit", disabled: disabled },
        "Wyślij"
      )
    ),
    error ? React.createElement("div", { className: "col-12" }, React.createElement("small", { className: "text-danger" }, error)) : null
  );
}

function PostList({ posts, onDelete, onAddComment }) {
  const [selectedPost, setSelectedPost] = useState(null);

  if (!posts.length) {
    return React.createElement(
      "div",
      { className: "alert alert-info text-center" },
      React.createElement("i", { className: "bi bi-info-circle me-2" }),
      "Brak wpisów. Dodaj coś nowego!"
    );
  }

  const isAdmin = currentUser?.role === "Admin";

  const openPost = (post) => {
    setSelectedPost(post);
  };

  const closePost = () => {
    setSelectedPost(null);
  };

  const truncateText = (text, maxLength = 150) => {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + "...";
  };

  return React.createElement(
    React.Fragment,
    null,
    React.createElement(
      "div",
      { className: "row g-4" },
      posts.map((post) => {
        return React.createElement(
          "div",
          { key: post.id, className: "col-12" },
          React.createElement(
            "div",
            { className: "card shadow-sm h-100", style: { cursor: "pointer" }, onClick: () => openPost(post) },
            post.imageUrl ? React.createElement("img", { src: post.imageUrl, className: "card-img-top", style: { height: "200px", objectFit: "cover" }, alt: post.title }) : null,
            React.createElement(
              "div",
              { className: "card-body" },
              React.createElement(
                "div",
                { className: "d-flex justify-content-between align-items-start mb-2" },
                React.createElement(
                  "div",
                  { style: { flex: 1 } },
                  React.createElement("small", { className: "text-muted d-block" }, React.createElement("i", { className: "bi bi-clock me-1" }), new Date(post.createdAt).toLocaleString()),
                  React.createElement("h5", { className: "card-title mb-1" }, post.title),
                  React.createElement("small", { className: "text-muted" }, React.createElement("i", { className: "bi bi-person me-1" }), post.author || "Anon")
                ),
                isAdmin ? React.createElement(
                  "button",
                  { className: "btn btn-sm btn-outline-danger", onClick: (e) => { e.stopPropagation(); onDelete(post.id); } },
                  React.createElement("i", { className: "bi bi-trash" })
                ) : null
              ),
              React.createElement("p", { className: "card-text" }, truncateText(post.content)),
              React.createElement(
                "div",
                { className: "d-flex justify-content-between align-items-center mt-3" },
                React.createElement(
                  "span",
                  { className: "text-muted small" },
                  React.createElement("i", { className: "bi bi-chat-dots me-1" }),
                  post.comments?.length ?? 0,
                  " ",
                  post.comments?.length === 1 ? "komentarz" : "komentarzy"
                ),
                React.createElement(
                  "span",
                  { className: "btn btn-link btn-sm p-0" },
                  "Czytaj więcej ",
                  React.createElement("i", { className: "bi bi-arrow-right" })
                )
              )
            )
          )
        );
      })
    ),
    
    selectedPost ? React.createElement(
      "div",
      { className: "modal-backdrop show", onClick: closePost }
    ) : null,
    
    selectedPost ? React.createElement(
      "div",
      { className: "modal show d-block", tabIndex: "-1", style: { overflowY: "auto" } },
      React.createElement(
        "div",
        { className: "modal-dialog modal-xl modal-dialog-scrollable" },
        React.createElement(
          "div",
          { className: "modal-content", onClick: (e) => e.stopPropagation() },
          React.createElement(
            "div",
            { className: "modal-header" },
            React.createElement(
              "div",
              null,
              React.createElement("h3", { className: "modal-title" }, selectedPost.title),
              React.createElement(
                "div",
                { className: "mt-2" },
                React.createElement("small", { className: "text-muted me-3" }, React.createElement("i", { className: "bi bi-clock me-1" }), new Date(selectedPost.createdAt).toLocaleString()),
                React.createElement("small", { className: "text-muted" }, React.createElement("i", { className: "bi bi-person me-1" }), selectedPost.author || "Anon")
              )
            ),
            React.createElement(
              "button",
              { type: "button", className: "btn-close", onClick: closePost, "aria-label": "Close" }
            )
          ),
          React.createElement(
            "div",
            { className: "modal-body" },
            selectedPost.imageUrl ? React.createElement("img", { src: selectedPost.imageUrl, className: "img-fluid rounded mb-4", style: { maxHeight: "500px", width: "100%", objectFit: "contain" }, alt: selectedPost.title }) : null,
            React.createElement("p", { className: "fs-5 lh-lg", style: { whiteSpace: "pre-wrap" } }, selectedPost.content),
            React.createElement("hr", { className: "my-4" }),
            React.createElement(
              "div",
              null,
              React.createElement(
                "h4",
                { className: "mb-4" },
                React.createElement("i", { className: "bi bi-chat-dots me-2" }),
                "Komentarze (",
                selectedPost.comments?.length ?? 0,
                ")"
              ),
              selectedPost.comments?.length
                ? React.createElement(
                    "div",
                    { className: "mb-4" },
                    selectedPost.comments.map((c) =>
                      React.createElement(
                        "div",
                        { key: c.id, className: "card mb-3" },
                        React.createElement(
                          "div",
                          { className: "card-body" },
                          React.createElement(
                            "div",
                            { className: "d-flex justify-content-between mb-2" },
                            React.createElement("strong", null, c.author || "Anon"),
                            React.createElement("small", { className: "text-muted" }, new Date(c.createdAt).toLocaleString())
                          ),
                          React.createElement("p", { className: "mb-0" }, c.content)
                        )
                      )
                    )
                  )
                : React.createElement("p", { className: "text-muted mb-4" }, "Brak komentarzy. Dodaj pierwszy!"),
              React.createElement(CommentForm, {
                onAdd: (payload) => onAddComment(selectedPost.id, payload),
              })
            )
          )
        )
      )
    ) : null
  );
}

function AboutPage() {
  return React.createElement(
    "div",
    { className: "card shadow-sm" },
    React.createElement(
      "div",
      { className: "card-body p-5" },
      React.createElement("h2", { className: "mb-4" }, "O mnie"),
      React.createElement("p", { className: "lead mb-4" }, "Witaj na moim blogu!"),
      React.createElement(
        "p",
        null,
        "Jestem pasjonatem programowania, który uwielbia dzielić się wiedzą i doświadczeniem. Na tym blogu znajdziesz artykuły o technologii, programowaniu i nie tylko."
      ),
      React.createElement(
        "p",
        null,
        "Blog został zbudowany przy użyciu:",
        React.createElement(
          "ul",
          { className: "mt-2" },
          React.createElement("li", null, "React 18 - frontend"),
          React.createElement("li", null, ".NET 9 + Aspire - backend"),
          React.createElement("li", null, "Bootstrap 5 - stylowanie"),
          React.createElement("li", null, "SQLite - baza danych")
        )
      )
    )
  );
}

function ShopPage() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [cart, setCart] = useState([]);
  const [showCart, setShowCart] = useState(false);
  const [showCheckout, setShowCheckout] = useState(false);
  const isAdmin = currentUser?.role === "Admin";

  useEffect(() => {
    loadProducts();
    const savedCart = localStorage.getItem("cart");
    if (savedCart) {
      setCart(JSON.parse(savedCart));
    }
  }, []);

  const loadProducts = async () => {
    setLoading(true);
    try {
      const data = await request("/products");
      const normalized = Array.isArray(data)
        ? data
        : Array.isArray(data?.items)
        ? data.items
        : [];
      setProducts(normalized);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const addToCart = (product, quantity = 1) => {
    setCart((prevCart) => {
      const existingItem = prevCart.find((item) => item.id === product.id);
      let newCart;
      if (existingItem) {
        newCart = prevCart.map((item) =>
          item.id === product.id ? { ...item, quantity: item.quantity + quantity } : item
        );
      } else {
        newCart = [...prevCart, { ...product, quantity }];
      }
      localStorage.setItem("cart", JSON.stringify(newCart));
      return newCart;
    });
  };

  const removeFromCart = (productId) => {
    setCart((prevCart) => {
      const newCart = prevCart.filter((item) => item.id !== productId);
      localStorage.setItem("cart", JSON.stringify(newCart));
      return newCart;
    });
  };

  const updateCartQuantity = (productId, quantity) => {
    if (quantity <= 0) {
      removeFromCart(productId);
      return;
    }
    setCart((prevCart) => {
      const newCart = prevCart.map((item) =>
        item.id === productId ? { ...item, quantity } : item
      );
      localStorage.setItem("cart", JSON.stringify(newCart));
      return newCart;
    });
  };

  const clearCart = () => {
    setCart([]);
    localStorage.removeItem("cart");
  };

  const cartTotal = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);
  const cartItemCount = cart.reduce((sum, item) => sum + item.quantity, 0);

  if (loading) {
    return React.createElement("div", { className: "text-center p-5" }, "Ładowanie produktów...");
  }

  if (error) {
    return React.createElement(
      "div",
      { className: "alert alert-danger" },
      React.createElement("i", { className: "bi bi-exclamation-triangle me-2" }),
      error
    );
  }

  if (showCheckout) {
    return React.createElement(CheckoutPage, {
      cart,
      cartTotal,
      onBack: () => setShowCheckout(false),
      onSuccess: () => {
        clearCart();
        setShowCheckout(false);
        setShowCart(false);
      },
    });
  }

  if (showCart) {
    return React.createElement(CartPage, {
      cart,
      cartTotal,
      onUpdateQuantity: updateCartQuantity,
      onRemove: removeFromCart,
      onBack: () => setShowCart(false),
      onCheckout: () => setShowCheckout(true),
    });
  }

  if (selectedProduct) {
    return React.createElement(ProductDetail, {
      product: selectedProduct,
      onBack: () => setSelectedProduct(null),
      onAddToCart: addToCart,
    });
  }

  if (isAdmin) {
    return React.createElement(AdminProductsPage, {
      products,
      onRefresh: loadProducts,
    });
  }

  return React.createElement(
    React.Fragment,
    null,
    React.createElement(
      "div",
      { className: "d-flex justify-content-between align-items-center mb-4" },
      React.createElement("h2", { className: "mb-0" }, "Sklep"),
      cartItemCount > 0
        ? React.createElement(
            "button",
            { className: "btn btn-primary position-relative", onClick: () => setShowCart(true) },
            React.createElement("i", { className: "bi bi-cart3 me-2" }),
            "Koszyk",
            React.createElement(
              "span",
              { className: "position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger" },
              cartItemCount
            )
          )
        : null
    ),
    products.length === 0
      ? React.createElement(
          "div",
          { className: "alert alert-info" },
          React.createElement("i", { className: "bi bi-info-circle me-2" }),
          "Brak produktów w sklepie."
        )
      : React.createElement(
          "div",
          { className: "row g-4" },
          products.map((product) =>
            React.createElement(
              "div",
              { key: product.id, className: "col-md-6 col-lg-4" },
              React.createElement(
                "div",
                { className: "card shadow-sm h-100" },
                product.imageUrl
                  ? React.createElement("img", {
                      src: product.imageUrl,
                      className: "card-img-top",
                      style: { height: "250px", objectFit: "cover", cursor: "pointer" },
                      alt: product.name,
                      onClick: () => setSelectedProduct(product),
                    })
                  : null,
                React.createElement(
                  "div",
                  { className: "card-body d-flex flex-column" },
                  React.createElement(
                    "h5",
                    { className: "card-title", style: { cursor: "pointer" }, onClick: () => setSelectedProduct(product) },
                    product.name
                  ),
                  React.createElement(
                    "p",
                    { className: "card-text text-muted flex-grow-1" },
                    product.description.length > 80
                      ? product.description.substring(0, 80) + "..."
                      : product.description
                  ),
                  React.createElement(
                    "div",
                    { className: "d-flex justify-content-between align-items-center mt-3" },
                    React.createElement("span", { className: "h5 mb-0 text-primary" }, product.price.toFixed(2), " PLN"),
                    product.stock > 0
                      ? React.createElement(
                          "button",
                          {
                            className: "btn btn-outline-primary btn-sm",
                            onClick: () => addToCart(product),
                          },
                          React.createElement("i", { className: "bi bi-cart-plus me-1" }),
                          "Do koszyka"
                        )
                      : React.createElement("span", { className: "badge bg-danger" }, "Brak w magazynie")
                  ),
                  React.createElement(
                    "small",
                    { className: "text-muted mt-2" },
                    React.createElement("i", { className: "bi bi-box me-1" }),
                    "Dostępne: ",
                    product.stock
                  )
                )
              )
            )
          )
        )
  );
}

function ProductDetail({ product, onBack, onAddToCart }) {
  const [quantity, setQuantity] = useState(1);

  const handleAddToCart = () => {
    onAddToCart(product, quantity);
    onBack();
  };

  return React.createElement(
    "div",
    null,
    React.createElement(
      "button",
      { className: "btn btn-outline-secondary mb-3", onClick: onBack },
      React.createElement("i", { className: "bi bi-arrow-left me-2" }),
      "Powrót do sklepu"
    ),
    React.createElement(
      "div",
      { className: "card shadow-sm" },
      React.createElement(
        "div",
        { className: "row g-0" },
        product.imageUrl
          ? React.createElement(
              "div",
              { className: "col-md-6" },
              React.createElement("img", {
                src: product.imageUrl,
                className: "img-fluid rounded-start",
                style: { height: "100%", objectFit: "cover" },
                alt: product.name,
              })
            )
          : null,
        React.createElement(
          "div",
          { className: "col-md-6" },
          React.createElement(
            "div",
            { className: "card-body p-4" },
            React.createElement("h2", { className: "card-title mb-3" }, product.name),
            React.createElement(
              "div",
              { className: "mb-3" },
              React.createElement("span", { className: "badge bg-primary me-2" }, product.category),
              product.stock > 0
                ? React.createElement("span", { className: "badge bg-success" }, "Dostępny")
                : React.createElement("span", { className: "badge bg-danger" }, "Brak w magazynie")
            ),
            React.createElement("p", { className: "card-text mb-4" }, product.description),
            React.createElement("h3", { className: "text-primary mb-4" }, product.price.toFixed(2), " PLN"),
            React.createElement(
              "div",
              { className: "mb-3" },
              React.createElement("small", { className: "text-muted" }, "Dostępne sztuki: ", product.stock)
            ),
            product.stock > 0
              ? React.createElement(
                  "div",
                  { className: "d-flex gap-2 align-items-center" },
                  React.createElement(
                    "div",
                    { className: "input-group", style: { width: "150px" } },
                    React.createElement(
                      "button",
                      {
                        className: "btn btn-outline-secondary",
                        onClick: () => setQuantity(Math.max(1, quantity - 1)),
                      },
                      "-"
                    ),
                    React.createElement("input", {
                      type: "number",
                      className: "form-control text-center",
                      value: quantity,
                      onChange: (e) => setQuantity(Math.max(1, Math.min(product.stock, parseInt(e.target.value) || 1))),
                      min: 1,
                      max: product.stock,
                    }),
                    React.createElement(
                      "button",
                      {
                        className: "btn btn-outline-secondary",
                        onClick: () => setQuantity(Math.min(product.stock, quantity + 1)),
                      },
                      "+"
                    )
                  ),
                  React.createElement(
                    "button",
                    { className: "btn btn-primary", onClick: handleAddToCart },
                    React.createElement("i", { className: "bi bi-cart-plus me-2" }),
                    "Dodaj do koszyka"
                  )
                )
              : null
          )
        )
      )
    )
  );
}

function CartPage({ cart, cartTotal, onUpdateQuantity, onRemove, onBack, onCheckout }) {
  if (cart.length === 0) {
    return React.createElement(
      "div",
      null,
      React.createElement(
        "button",
        { className: "btn btn-outline-secondary mb-3", onClick: onBack },
        React.createElement("i", { className: "bi bi-arrow-left me-2" }),
        "Powrót do sklepu"
      ),
      React.createElement(
        "div",
        { className: "alert alert-info text-center" },
        React.createElement("i", { className: "bi bi-cart-x me-2" }),
        "Koszyk jest pusty"
      )
    );
  }

  return React.createElement(
    "div",
    null,
    React.createElement(
      "button",
      { className: "btn btn-outline-secondary mb-3", onClick: onBack },
      React.createElement("i", { className: "bi bi-arrow-left me-2" }),
      "Powrót do sklepu"
    ),
    React.createElement("h2", { className: "mb-4" }, "Koszyk"),
    React.createElement(
      "div",
      { className: "card shadow-sm" },
      React.createElement(
        "div",
        { className: "card-body" },
        cart.map((item) =>
          React.createElement(
            "div",
            { key: item.id, className: "row align-items-center border-bottom pb-3 mb-3" },
            React.createElement(
              "div",
              { className: "col-md-2 col-3" },
              item.imageUrl
                ? React.createElement("img", {
                    src: item.imageUrl,
                    className: "img-fluid rounded",
                    alt: item.name,
                    style: { maxHeight: "80px", objectFit: "cover" }
                  })
                : React.createElement("div", {
                    className: "bg-light rounded d-flex align-items-center justify-content-center",
                    style: { height: "80px" }
                  }, React.createElement("i", { className: "bi bi-image text-muted", style: { fontSize: "2rem" } }))
            ),
            React.createElement(
              "div",
              { className: "col-md-3 col-9" },
              React.createElement("h6", { className: "mb-1" }, item.name),
              React.createElement("small", { className: "text-muted" }, item.price.toFixed(2), " PLN / szt.")
            ),
            React.createElement(
              "div",
              { className: "col-md-3 col-6 mt-2 mt-md-0" },
              React.createElement(
                "div",
                { className: "input-group input-group-sm" },
                React.createElement(
                  "button",
                  {
                    className: "btn btn-outline-secondary",
                    onClick: () => onUpdateQuantity(item.id, item.quantity - 1),
                  },
                  "-"
                ),
                React.createElement("input", {
                  type: "number",
                  className: "form-control text-center",
                  value: item.quantity,
                  onChange: (e) => onUpdateQuantity(item.id, parseInt(e.target.value) || 1),
                  min: 1,
                }),
                React.createElement(
                  "button",
                  {
                    className: "btn btn-outline-secondary",
                    onClick: () => onUpdateQuantity(item.id, item.quantity + 1),
                  },
                  "+"
                )
              )
            ),
            React.createElement(
              "div",
              { className: "col-md-2 col-3 text-end mt-2 mt-md-0" },
              React.createElement("strong", { style: { fontSize: "1.1rem" } }, (item.price * item.quantity).toFixed(2), " PLN")
            ),
            React.createElement(
              "div",
              { className: "col-md-2 col-3 text-end mt-2 mt-md-0" },
              React.createElement(
                "button",
                { className: "btn btn-sm btn-outline-danger", onClick: () => onRemove(item.id) },
                React.createElement("i", { className: "bi bi-trash" })
              )
            )
          )
        ),
        React.createElement(
          "div",
          { className: "d-flex justify-content-between align-items-center mt-4 pt-3 border-top" },
          React.createElement("h4", { className: "mb-0" }, "Suma:"),
          React.createElement("h4", { className: "mb-0 text-primary" }, cartTotal.toFixed(2), " PLN")
        ),
        React.createElement(
          "button",
          { className: "btn btn-primary w-100 mt-3", onClick: onCheckout },
          React.createElement("i", { className: "bi bi-credit-card me-2" }),
          "Przejdź do kasy"
        )
      )
    )
  );
}

function CheckoutPage({ cart, cartTotal, onBack, onSuccess }) {
  const [formData, setFormData] = useState({
    customerName: "",
    customerEmail: "",
    customerPhone: "",
    shippingAddress: "",
    shippingCity: "",
    shippingPostalCode: "",
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");

    try {
      const orderData = {
        ...formData,
        items: cart.map((item) => ({
          productId: item.id,
          quantity: item.quantity,
        })),
      };

      await request("/orders", {
        method: "POST",
        body: JSON.stringify(orderData),
      });

      alert("Zamówienie zostało złożone! Szczegóły zostały wysłane na email.");
      onSuccess();
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return React.createElement(
    "div",
    null,
    React.createElement(
      "button",
      { className: "btn btn-outline-secondary mb-3", onClick: onBack },
      React.createElement("i", { className: "bi bi-arrow-left me-2" }),
      "Powrót do koszyka"
    ),
    React.createElement("h2", { className: "mb-4" }, "Finalizacja zamówienia"),
    React.createElement(
      "div",
      { className: "row" },
      React.createElement(
        "div",
        { className: "col-md-8" },
        React.createElement(
          "div",
          { className: "card shadow-sm mb-4" },
          React.createElement(
            "div",
            { className: "card-body" },
            React.createElement("h4", { className: "card-title mb-3" }, "Dane do wysyłki"),
            error
              ? React.createElement("div", { className: "alert alert-danger" }, error)
              : null,
            React.createElement(
              "form",
              { onSubmit: handleSubmit },
              React.createElement(
                "div",
                { className: "mb-3" },
                React.createElement("label", { className: "form-label" }, "Imię i nazwisko *"),
                React.createElement("input", {
                  type: "text",
                  className: "form-control",
                  value: formData.customerName,
                  onChange: (e) => setFormData({ ...formData, customerName: e.target.value }),
                  required: true,
                })
              ),
              React.createElement(
                "div",
                { className: "mb-3" },
                React.createElement("label", { className: "form-label" }, "Email *"),
                React.createElement("input", {
                  type: "email",
                  className: "form-control",
                  value: formData.customerEmail,
                  onChange: (e) => setFormData({ ...formData, customerEmail: e.target.value }),
                  required: true,
                })
              ),
              React.createElement(
                "div",
                { className: "mb-3" },
                React.createElement("label", { className: "form-label" }, "Telefon *"),
                React.createElement("input", {
                  type: "tel",
                  className: "form-control",
                  value: formData.customerPhone,
                  onChange: (e) => setFormData({ ...formData, customerPhone: e.target.value }),
                  required: true,
                })
              ),
              React.createElement(
                "div",
                { className: "mb-3" },
                React.createElement("label", { className: "form-label" }, "Adres *"),
                React.createElement("input", {
                  type: "text",
                  className: "form-control",
                  value: formData.shippingAddress,
                  onChange: (e) => setFormData({ ...formData, shippingAddress: e.target.value }),
                  required: true,
                })
              ),
              React.createElement(
                "div",
                { className: "row" },
                React.createElement(
                  "div",
                  { className: "col-md-8 mb-3" },
                  React.createElement("label", { className: "form-label" }, "Miasto *"),
                  React.createElement("input", {
                    type: "text",
                    className: "form-control",
                    value: formData.shippingCity,
                    onChange: (e) => setFormData({ ...formData, shippingCity: e.target.value }),
                    required: true,
                  })
                ),
                React.createElement(
                  "div",
                  { className: "col-md-4 mb-3" },
                  React.createElement("label", { className: "form-label" }, "Kod pocztowy *"),
                  React.createElement("input", {
                    type: "text",
                    className: "form-control",
                    value: formData.shippingPostalCode,
                    onChange: (e) => setFormData({ ...formData, shippingPostalCode: e.target.value }),
                    required: true,
                  })
                )
              ),
              React.createElement(
                "button",
                { type: "submit", className: "btn btn-success w-100", disabled: submitting },
                submitting ? "Wysyłanie..." : "Złóż zamówienie"
              )
            )
          )
        )
      ),
      React.createElement(
        "div",
        { className: "col-md-4" },
        React.createElement(
          "div",
          { className: "card shadow-sm" },
          React.createElement(
            "div",
            { className: "card-body" },
            React.createElement("h5", { className: "card-title mb-3" }, "Podsumowanie"),
            cart.map((item) =>
              React.createElement(
                "div",
                { key: item.id, className: "d-flex justify-content-between align-items-start mb-3 pb-2 border-bottom" },
                React.createElement(
                  "div",
                  { className: "flex-grow-1 me-3" },
                  React.createElement(
                    "div",
                    { className: "fw-semibold mb-1", style: { fontSize: "0.95rem" } },
                    item.name
                  ),
                  React.createElement(
                    "small",
                    { className: "text-muted" },
                    "Ilość: ",
                    item.quantity,
                    " × ",
                    item.price.toFixed(2),
                    " PLN"
                  )
                ),
                React.createElement(
                  "div",
                  { className: "text-end flex-shrink-0" },
                  React.createElement("strong", null, (item.price * item.quantity).toFixed(2), " PLN")
                )
              )
            ),
            React.createElement("hr", null),
            React.createElement(
              "div",
              { className: "d-flex justify-content-between" },
              React.createElement("strong", null, "Suma:"),
              React.createElement("strong", { className: "text-primary" }, cartTotal.toFixed(2), " PLN")
            ),
            React.createElement(
              "div",
              { className: "alert alert-info mt-3 mb-0" },
              React.createElement("small", null, "Zamówienie zostanie wysłane na podany email. Płatność przy odbiorze.")
            )
          )
        )
      )
    )
  );
}

function AdminProductsPage({ products, onRefresh }) {
  const [showForm, setShowForm] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);

  const handleDelete = async (id) => {
    if (!confirm("Czy na pewno chcesz usunąć ten produkt?")) return;

    try {
      await request(`/products/${id}`, { method: "DELETE" });
      onRefresh();
    } catch (err) {
      alert(err.message);
    }
  };

  if (showForm) {
    return React.createElement(ProductForm, {
      product: editingProduct,
      onCancel: () => {
        setShowForm(false);
        setEditingProduct(null);
      },
      onSuccess: () => {
        setShowForm(false);
        setEditingProduct(null);
        onRefresh();
      },
    });
  }

  return React.createElement(
    "div",
    null,
    React.createElement(
      "div",
      { className: "d-flex justify-content-between align-items-center mb-4" },
      React.createElement("h2", { className: "mb-0" }, "Zarządzanie produktami"),
      React.createElement(
        "button",
        { className: "btn btn-primary", onClick: () => setShowForm(true) },
        React.createElement("i", { className: "bi bi-plus-circle me-2" }),
        "Dodaj produkt"
      )
    ),
    products.length === 0
      ? React.createElement(
          "div",
          { className: "alert alert-info" },
          "Brak produktów. Dodaj pierwszy produkt."
        )
      : React.createElement(
          "div",
          { className: "row g-4" },
          products.map((product) =>
            React.createElement(
              "div",
              { key: product.id, className: "col-md-6 col-lg-4" },
              React.createElement(
                "div",
                { className: "card shadow-sm h-100" },
                product.imageUrl
                  ? React.createElement("img", {
                      src: product.imageUrl,
                      className: "card-img-top",
                      style: { height: "200px", objectFit: "cover" },
                      alt: product.name,
                    })
                  : null,
                React.createElement(
                  "div",
                  { className: "card-body" },
                  React.createElement("h5", { className: "card-title" }, product.name),
                  React.createElement("p", { className: "card-text text-muted" }, product.description),
                  React.createElement(
                    "div",
                    { className: "d-flex justify-content-between align-items-center mb-2" },
                    React.createElement("span", { className: "h5 mb-0 text-primary" }, product.price.toFixed(2), " PLN"),
                    React.createElement("span", { className: "badge bg-secondary" }, "Stan: ", product.stock)
                  ),
                  React.createElement(
                    "div",
                    { className: "d-flex gap-2" },
                    React.createElement(
                      "button",
                      {
                        className: "btn btn-sm btn-outline-primary flex-grow-1",
                        onClick: () => {
                          setEditingProduct(product);
                          setShowForm(true);
                        },
                      },
                      React.createElement("i", { className: "bi bi-pencil me-1" }),
                      "Edytuj"
                    ),
                    React.createElement(
                      "button",
                      { className: "btn btn-sm btn-outline-danger", onClick: () => handleDelete(product.id) },
                      React.createElement("i", { className: "bi bi-trash" })
                    )
                  )
                )
              )
            )
          )
        )
  );
}

function ProductForm({ product, onCancel, onSuccess }) {
  const [formData, setFormData] = useState({
    name: product?.name || "",
    description: product?.description || "",
    price: product?.price || "",
    imageUrl: product?.imageUrl || "",
    category: product?.category || "",
    stock: product?.stock || 0,
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");

    try {
      const payload = {
        ...formData,
        price: parseFloat(formData.price),
        stock: parseInt(formData.stock),
      };

      if (product) {
        await request(`/products/${product.id}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
      } else {
        await request("/products", {
          method: "POST",
          body: JSON.stringify(payload),
        });
      }

      onSuccess();
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return React.createElement(
    "div",
    null,
    React.createElement(
      "button",
      { className: "btn btn-outline-secondary mb-3", onClick: onCancel },
      React.createElement("i", { className: "bi bi-arrow-left me-2" }),
      "Anuluj"
    ),
    React.createElement("h2", { className: "mb-4" }, product ? "Edytuj produkt" : "Dodaj produkt"),
    React.createElement(
      "div",
      { className: "card shadow-sm" },
      React.createElement(
        "div",
        { className: "card-body" },
        error ? React.createElement("div", { className: "alert alert-danger" }, error) : null,
        React.createElement(
          "form",
          { onSubmit: handleSubmit },
          React.createElement(
            "div",
            { className: "mb-3" },
            React.createElement("label", { className: "form-label" }, "Nazwa produktu *"),
            React.createElement("input", {
              type: "text",
              className: "form-control",
              value: formData.name,
              onChange: (e) => setFormData({ ...formData, name: e.target.value }),
              required: true,
            })
          ),
          React.createElement(
            "div",
            { className: "mb-3" },
            React.createElement("label", { className: "form-label" }, "Opis *"),
            React.createElement("textarea", {
              className: "form-control",
              rows: 4,
              value: formData.description,
              onChange: (e) => setFormData({ ...formData, description: e.target.value }),
              required: true,
            })
          ),
          React.createElement(
            "div",
            { className: "row" },
            React.createElement(
              "div",
              { className: "col-md-6 mb-3" },
              React.createElement("label", { className: "form-label" }, "Cena (PLN) *"),
              React.createElement("input", {
                type: "number",
                step: "0.01",
                className: "form-control",
                value: formData.price,
                onChange: (e) => setFormData({ ...formData, price: e.target.value }),
                required: true,
              })
            ),
            React.createElement(
              "div",
              { className: "col-md-6 mb-3" },
              React.createElement("label", { className: "form-label" }, "Stan magazynowy *"),
              React.createElement("input", {
                type: "number",
                className: "form-control",
                value: formData.stock,
                onChange: (e) => setFormData({ ...formData, stock: e.target.value }),
                required: true,
              })
            )
          ),
          React.createElement(
            "div",
            { className: "mb-3" },
            React.createElement("label", { className: "form-label" }, "Kategoria *"),
            React.createElement("input", {
              type: "text",
              className: "form-control",
              value: formData.category,
              onChange: (e) => setFormData({ ...formData, category: e.target.value }),
              required: true,
            })
          ),
          React.createElement(
            "div",
            { className: "mb-3" },
            React.createElement("label", { className: "form-label" }, "URL obrazka"),
            React.createElement("input", {
              type: "url",
              className: "form-control",
              value: formData.imageUrl,
              onChange: (e) => setFormData({ ...formData, imageUrl: e.target.value }),
            })
          ),
          React.createElement(
            "button",
            { type: "submit", className: "btn btn-primary", disabled: submitting },
            submitting ? "Zapisywanie..." : product ? "Zapisz zmiany" : "Dodaj produkt"
          )
        )
      )
    )
  );
}

function ContactPage() {
  const [formData, setFormData] = useState({ name: "", email: "", message: "" });
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = (e) => {
    e.preventDefault();
    setSubmitted(true);
    setTimeout(() => {
      setFormData({ name: "", email: "", message: "" });
      setSubmitted(false);
    }, 3000);
  };

  return React.createElement(
    "div",
    { className: "card shadow-sm" },
    React.createElement(
      "div",
      { className: "card-body p-5" },
      React.createElement("h2", { className: "mb-4" }, "Kontakt"),
      React.createElement("p", { className: "mb-4" }, "Masz pytania? Skontaktuj się ze mną!"),
      submitted
        ? React.createElement(
            "div",
            { className: "alert alert-success" },
            React.createElement("i", { className: "bi bi-check-circle me-2" }),
            "Dziękujemy za wiadomość! Odpowiemy wkrótce."
          )
        : React.createElement(
            "form",
            { onSubmit: handleSubmit },
            React.createElement(
              "div",
              { className: "mb-3" },
              React.createElement("label", { className: "form-label" }, "Imię i nazwisko"),
              React.createElement("input", {
                type: "text",
                className: "form-control",
                value: formData.name,
                onChange: (e) => setFormData({ ...formData, name: e.target.value }),
                required: true,
              })
            ),
            React.createElement(
              "div",
              { className: "mb-3" },
              React.createElement("label", { className: "form-label" }, "Email"),
              React.createElement("input", {
                type: "email",
                className: "form-control",
                value: formData.email,
                onChange: (e) => setFormData({ ...formData, email: e.target.value }),
                required: true,
              })
            ),
            React.createElement(
              "div",
              { className: "mb-3" },
              React.createElement("label", { className: "form-label" }, "Wiadomość"),
              React.createElement("textarea", {
                className: "form-control",
                rows: 5,
                value: formData.message,
                onChange: (e) => setFormData({ ...formData, message: e.target.value }),
                required: true,
              })
            ),
            React.createElement(
              "button",
              { type: "submit", className: "btn btn-primary" },
              React.createElement("i", { className: "bi bi-send me-2" }),
              "Wyślij wiadomość"
            )
          )
    )
  );
}

function App() {
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [user, setUser] = useState(currentUser);
  const [showRegister, setShowRegister] = useState(false);
  const [authMessage, setAuthMessage] = useState("");
  const [activeTab, setActiveTab] = useState("home");

  const handleLogin = (loggedInUser) => {
    setUser(loggedInUser);
    refresh();
  };

  const handleLogout = () => {
    authToken = null;
    currentUser = null;
    localStorage.removeItem("authToken");
    localStorage.removeItem("currentUser");
    setUser(null);
    setShowRegister(false);
    setAuthMessage("");
  };

  const handleRegisterSuccess = () => {
    setAuthMessage("Konto zostało utworzone. Zaloguj się.");
    setShowRegister(false);
  };

  const handleShowRegister = () => {
    setAuthMessage("");
    setShowRegister(true);
  };

  const refresh = async () => {
    setError("");
    try {
      const data = await request("/posts");
      const normalized = Array.isArray(data)
        ? data
        : Array.isArray(data?.items)
        ? data.items
        : [];
      setPosts(normalized);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refresh();
  }, []);

  const handleCreate = async (payload) => {
    setSubmitting(true);
    try {
      await request("/posts", { method: "POST", body: JSON.stringify(payload) });
      await refresh();
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id) => {
    try {
      await request(`/posts/${id}`, { method: "DELETE" });
      setPosts((prev) => prev.filter((p) => p.id !== id));
    } catch (err) {
      setError(err.message);
    }
  };

  const handleAddComment = async (postId, payload) => {
    setError("");
    const newComment = await request(`/posts/${postId}/comments`, {
      method: "POST",
      body: JSON.stringify(payload),
    });

    setPosts((prev) =>
      prev.map((p) =>
        p.id === postId
          ? { ...p, comments: [newComment, ...(p.comments || [])] }
          : p
      )
    );
  };
  
  const onAddCommentAndUpdateModal = async (postId, payload) => {
    await handleAddComment(postId, payload);
    // Refresh selectedPost from updated posts state
    setPosts((currentPosts) => {
      const updatedPost = currentPosts.find(p => p.id === postId);
      if (updatedPost) {
        setSelectedPost(updatedPost);
      }
      return currentPosts;
    });
  };

  const headline = useMemo(() => {
    if (loading) return "Ładowanie wpisów...";
    if (error) return "Ups! Coś poszło nie tak.";
    return posts.length ? `Masz ${posts.length} wpis(y)` : "Zacznij od pierwszego wpisu";
  }, [loading, error, posts.length]);

  if (!user) {
    return React.createElement(
      "div",
      { className: "container py-5" },
      React.createElement(
        "div",
        { className: "text-center mb-5" },
        React.createElement("p", { className: "text-primary text-uppercase fw-bold mb-2" }, "SimpleBlog x Aspire"),
        React.createElement("h1", { className: "display-4 fw-bold mb-3" }, "Twój lekki blog na React + .NET"),
        React.createElement("p", { className: "text-muted" }, "Zaloguj się, aby kontynuować")
      ),
      authMessage ? React.createElement("div", { className: "alert alert-success text-center" }, authMessage) : null,
      showRegister
        ? React.createElement(RegisterForm, { onRegistered: handleRegisterSuccess, onCancel: () => setShowRegister(false) })
        : React.createElement(LoginForm, { onLogin: handleLogin, onShowRegister: handleShowRegister })
    );
  }

  return React.createElement(
    "div",
    { className: "container py-4" },
    React.createElement(
      "header",
      { className: "mb-4" },
      React.createElement(
        "div",
        { className: "text-center mb-4" },
        React.createElement("p", { className: "text-primary text-uppercase fw-bold mb-2" }, "SimpleBlog x Aspire"),
        React.createElement("h1", { className: "display-5 fw-bold mb-3" }, "Twój lekki blog na React + .NET"),
        React.createElement("p", { className: "text-muted" }, "API działa w osobnej usłudze; frontend korzysta z niego przez warstwę proxy.")
      ),
      React.createElement(
        "div",
        { className: "d-flex justify-content-between align-items-center p-3 bg-light rounded" },
        React.createElement(
          "div",
          null,
          React.createElement("span", { className: "text-muted me-2" }, "Zalogowany:"),
          React.createElement("strong", null, user.username, " "),
          React.createElement("span", { className: "badge bg-primary" }, user.role)
        ),
        React.createElement(
          "button",
          { className: "btn btn-outline-secondary btn-sm", onClick: handleLogout },
          React.createElement("i", { className: "bi bi-box-arrow-right me-1" }),
          "Wyloguj"
        )
      ),
      activeTab === "home" ? React.createElement(
        React.Fragment,
        null,
        React.createElement(
          "div",
          { className: "alert alert-info mt-3" },
          React.createElement("i", { className: "bi bi-info-circle me-2" }),
          headline
        ),
        error ? React.createElement("div", { className: "alert alert-danger mt-3" }, error) : null
      ) : null
    ),

    React.createElement(
      "ul",
      { className: "nav nav-tabs mb-4" },
      React.createElement(
        "li",
        { className: "nav-item" },
        React.createElement(
          "button",
          {
            className: `nav-link ${activeTab === "home" ? "active" : ""}`,
            onClick: () => setActiveTab("home")
          },
          React.createElement("i", { className: "bi bi-house-door me-2" }),
          "Home"
        )
      ),
      React.createElement(
        "li",
        { className: "nav-item" },
        React.createElement(
          "button",
          {
            className: `nav-link ${activeTab === "about" ? "active" : ""}`,
            onClick: () => setActiveTab("about")
          },
          React.createElement("i", { className: "bi bi-person me-2" }),
          "O mnie"
        )
      ),
      React.createElement(
        "li",
        { className: "nav-item" },
        React.createElement(
          "button",
          {
            className: `nav-link ${activeTab === "shop" ? "active" : ""}`,
            onClick: () => setActiveTab("shop")
          },
          React.createElement("i", { className: "bi bi-shop me-2" }),
          "Sklep"
        )
      ),
      React.createElement(
        "li",
        { className: "nav-item" },
        React.createElement(
          "button",
          {
            className: `nav-link ${activeTab === "contact" ? "active" : ""}`,
            onClick: () => setActiveTab("contact")
          },
          React.createElement("i", { className: "bi bi-envelope me-2" }),
          "Kontakt"
        )
      )
    ),

    React.createElement(
      "button",
      {
        className: "theme-toggle",
        onClick: () => {
          const currentTheme = getTheme();
          const newTheme = currentTheme === "light" ? "dark" : "light";
          setTheme(newTheme);
          window.location.reload();
        },
        title: "Przełącz motyw"
      },
      React.createElement("i", { className: getTheme() === "light" ? "bi bi-moon-stars-fill" : "bi bi-sun-fill" })
    ),

    React.createElement(
      "main",
      null,
      activeTab === "home" ? React.createElement(
        React.Fragment,
        null,
        user.role === "Admin" ? React.createElement("div", { className: "mb-4" }, React.createElement(PostForm, { onCreated: handleCreate, isSubmitting: submitting })) : null,
        React.createElement(
          "div",
          null,
          React.createElement(
            "div",
            { className: "d-flex justify-content-between align-items-center mb-3" },
            React.createElement("h2", { className: "mb-0" }, "Wpisy"),
            React.createElement(
              "button",
              { className: "btn btn-outline-primary btn-sm", onClick: refresh, disabled: loading },
              React.createElement("i", { className: "bi bi-arrow-clockwise me-1" }),
              "Odśwież"
            )
          ),
          loading
            ? React.createElement("p", { className: "text-muted" }, "Ładowanie...")
            : React.createElement(PostList, {
                posts: posts,
                onDelete: handleDelete,
                onAddComment: onAddCommentAndUpdateModal,
              })
        )
      ) : null,
      activeTab === "about" ? React.createElement(AboutPage) : null,
      activeTab === "shop" ? React.createElement(ShopPage) : null,
      activeTab === "contact" ? React.createElement(ContactPage) : null
    )
  );
}

const container = document.getElementById("app");
const root = ReactDOM.createRoot(container);
root.render(React.createElement(App));
