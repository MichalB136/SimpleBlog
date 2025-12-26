const { useEffect, useMemo, useState } = React;

const apiBase = "/api";

let authToken = localStorage.getItem("authToken") || null;
let currentUser = JSON.parse(localStorage.getItem("currentUser") || "null");

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

function LoginForm({ onLogin }) {
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
    { className: "card", style: { maxWidth: "400px", margin: "0 auto" } },
    React.createElement("h2", null, "Zaloguj się"),
    React.createElement("p", { className: "muted" }, "Admin: admin/admin123 | User: user/user123"),
    React.createElement(
      "form",
      { onSubmit: handleSubmit },
      React.createElement(
        "label",
        { className: "field" },
        React.createElement("span", null, "Nazwa użytkownika"),
        React.createElement("input", {
          value: username,
          onChange: (e) => setUsername(e.target.value),
          required: true,
        })
      ),
      React.createElement(
        "label",
        { className: "field" },
        React.createElement("span", null, "Hasło"),
        React.createElement("input", {
          type: "password",
          value: password,
          onChange: (e) => setPassword(e.target.value),
          required: true,
        })
      ),
      React.createElement(
        "button",
        { type: "submit", className: "button", disabled: loading },
        loading ? "Logowanie..." : "Zaloguj"
      ),
      error ? React.createElement("p", { className: "error" }, error) : null
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
    "form",
    { className: "card", onSubmit: handleSubmit },
    React.createElement(
      "div",
      { className: "card__header" },
      React.createElement(
        "div",
        null,
        React.createElement("p", { className: "eyebrow" }, "Nowy wpis"),
        React.createElement("h2", null, "Dodaj notkę")
      ),
      React.createElement(
        "button",
        { type: "submit", className: "button", disabled: isSubmitting },
        isSubmitting ? "Zapisywanie..." : "Publikuj"
      )
    ),
    React.createElement(
      "label",
      { className: "field" },
      React.createElement("span", null, "Tytuł"),
      React.createElement("input", {
        value: title,
        onChange: (e) => setTitle(e.target.value),
        placeholder: "Mój pierwszy wpis",
        required: true,
      })
    ),
    React.createElement(
      "label",
      { className: "field" },
      React.createElement("span", null, "Autor"),
      React.createElement("input", {
        value: author,
        onChange: (e) => setAuthor(e.target.value),
        placeholder: "Twoje imię",
      })
    ),
    React.createElement(
      "label",
      { className: "field" },
      React.createElement("span", null, "Treść"),
      React.createElement("textarea", {
        rows: "5",
        value: content,
        onChange: (e) => setContent(e.target.value),
        placeholder: "Co masz dziś do powiedzenia?",
        required: true,
      })
    ),
    React.createElement(
      "label",
      { className: "field" },
      React.createElement("span", null, "Zdjęcie (opcjonalnie)"),
      React.createElement("input", {
        type: "file",
        accept: "image/*",
        onChange: handleImageChange,
      }),
      imageUrl && React.createElement("img", { src: imageUrl, style: { maxWidth: "100%", marginTop: "8px", borderRadius: "8px" } })
    ),
    error ? React.createElement("p", { className: "error" }, error) : null
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
    { className: "comment-form", onSubmit: submit },
    React.createElement("input", {
      placeholder: "Twoje imię",
      value: author,
      onChange: (e) => setAuthor(e.target.value),
      disabled: disabled,
    }),
    React.createElement("input", {
      placeholder: "Dodaj komentarz",
      value: content,
      onChange: (e) => setContent(e.target.value),
      disabled: disabled,
      required: true,
    }),
    React.createElement(
      "button",
      { className: "ghost", type: "submit", disabled: disabled },
      "Wyślij"
    ),
    error ? React.createElement("span", { className: "error" }, error) : null
  );
}

function PostList({ posts, onDelete, onAddComment }) {
  if (!posts.length) {
    return React.createElement("p", { className: "muted" }, "Brak wpisów. Dodaj coś nowego!");
  }

  const isAdmin = currentUser?.role === "Admin";

  return React.createElement(
    "div",
    { className: "grid" },
    posts.map((post) =>
      React.createElement(
        "article",
        { key: post.id, className: "card" },
        React.createElement(
          "div",
          { className: "card__header" },
          React.createElement(
            "div",
            null,
            React.createElement(
              "p",
              { className: "eyebrow" },
              new Date(post.createdAt).toLocaleString()
            ),
            React.createElement("h3", null, post.title)
          ),
          isAdmin ? React.createElement(
            "button",
            { className: "ghost", onClick: () => onDelete(post.id) },
            "Usuń"
          ) : null
        ),
        post.imageUrl ? React.createElement("img", { src: post.imageUrl, style: { width: "100%", borderRadius: "8px", marginBottom: "12px" }, alt: post.title }) : null,
        React.createElement("p", { className: "muted" }, "Autor: ", post.author || "Anon"),
        React.createElement("p", { className: "content" }, post.content),

        React.createElement(
          "div",
          { className: "comments" },
          React.createElement(
            "div",
            { className: "comments__header" },
            React.createElement(
              "p",
              { className: "eyebrow" },
              "Komentarze (",
              post.comments?.length ?? 0,
              ")"
            )
          ),
          post.comments?.length
            ? post.comments.map((c) =>
                React.createElement(
                  "div",
                  { key: c.id, className: "comment" },
                  React.createElement(
                    "div",
                    { className: "comment__meta" },
                    React.createElement(
                      "span",
                      { className: "comment__author" },
                      c.author || "Anon"
                    ),
                    React.createElement(
                      "span",
                      { className: "comment__date" },
                      new Date(c.createdAt).toLocaleString()
                    )
                  ),
                  React.createElement("p", { className: "comment__body" }, c.content)
                )
              )
            : React.createElement(
                "p",
                { className: "muted" },
                "Brak komentarzy. Dodaj pierwszy!"
              ),
          React.createElement(CommentForm, {
            onAdd: (payload) => onAddComment(post.id, payload),
          })
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
  };

  const refresh = async () => {
    setError("");
    try {
      const data = await request("/posts");
      setPosts(Array.isArray(data) ? data : []);
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

  const headline = useMemo(() => {
    if (loading) return "Ładowanie wpisów...";
    if (error) return "Ups! Coś poszło nie tak.";
    return posts.length ? `Masz ${posts.length} wpis(y)` : "Zacznij od pierwszego wpisu";
  }, [loading, error, posts.length]);

  if (!user) {
    return React.createElement(
      "div",
      { className: "layout" },
      React.createElement(
        "header",
        { className: "hero" },
        React.createElement("p", { className: "eyebrow" }, "SimpleBlog x Aspire"),
        React.createElement("h1", null, "Twój lekki blog na React + .NET"),
        React.createElement("p", { className: "muted" }, "Zaloguj się, aby kontynuować")
      ),
      React.createElement("main", { style: { marginTop: "24px" } }, React.createElement(LoginForm, { onLogin: handleLogin }))
    );
  }

  return React.createElement(
    "div",
    { className: "layout" },
    React.createElement(
      "header",
      { className: "hero" },
      React.createElement("p", { className: "eyebrow" }, "SimpleBlog x Aspire"),
      React.createElement("h1", null, "Twój lekki blog na React + .NET"),
      React.createElement("p", { className: "muted" }, "API działa w osobnej usłudze; frontend korzysta z niego przez warstwę proxy."),
      React.createElement("div", { className: "hero__status" }, headline),
      React.createElement(
        "div",
        { style: { marginTop: "12px" } },
        React.createElement("span", { className: "muted" }, "Zalogowany: "),
        React.createElement("strong", null, user.username, " (", user.role, ")"),
        React.createElement(
          "button",
          { className: "ghost", onClick: handleLogout, style: { marginLeft: "12px" } },
          "Wyloguj"
        )
      ),
      error ? React.createElement("p", { className: "error" }, error) : null
    ),

    React.createElement(
      "main",
      { className: user.role === "Admin" ? "columns" : "" },
      user.role === "Admin" ? React.createElement(PostForm, { onCreated: handleCreate, isSubmitting: submitting }) : null,
      React.createElement(
        "section",
        null,
        React.createElement(
          "div",
          { className: "section__header" },
          React.createElement("h2", null, "Wpisy"),
          React.createElement(
            "button",
            { className: "ghost", onClick: refresh, disabled: loading },
            "Odśwież"
          )
        ),
        loading
          ? React.createElement("p", { className: "muted" }, "Ładowanie...")
          : React.createElement(PostList, {
              posts: posts,
              onDelete: handleDelete,
              onAddComment: handleAddComment,
            })
      )
    )
  );
}

const container = document.getElementById("app");
const root = ReactDOM.createRoot(container);
root.render(React.createElement(App));
