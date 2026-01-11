import { useState, useCallback, useEffect } from 'react';
import type { Post } from '@/types/post';
import { postsApi } from '@/api/posts';

export function usePosts() {
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const sortPosts = useCallback((items: Post[]) => {
    return [...items].sort(
      (a, b) =>
        Number(b.isPinned || false) - Number(a.isPinned || false) ||
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    );
  }, []);

  const refresh = useCallback(async () => {
    setError('');
    try {
      const data = await postsApi.getAll();
      // API returns paginated response with items array
      const normalized: Post[] = data.items && Array.isArray(data.items) ? data.items : [];
      setPosts(sortPosts(normalized));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load posts');
    } finally {
      setLoading(false);
    }
  }, [sortPosts]);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const create = useCallback(
    async (payload: any) => {
      try {
        await postsApi.create(payload);
        await refresh();
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to create post');
        throw err;
      }
    },
    [refresh]
  );

  const delete_ = useCallback(
    async (id: string) => {
      try {
        await postsApi.delete(id);
        setPosts((prev) => prev.filter((p) => p.id !== id));
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to delete post');
        throw err;
      }
    },
    []
  );

  const togglePin = useCallback(
    async (post: Post) => {
      try {
        const updated = post.isPinned
          ? await postsApi.unpin(post.id)
          : await postsApi.pin(post.id);
        setPosts((prev) => sortPosts(prev.map((p) => (p.id === post.id ? updated : p))));
        return updated;
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to toggle pin');
        throw err;
      }
    },
    [sortPosts]
  );

  const addComment = useCallback(
    async (postId: string, payload: any) => {
      try {
        const newComment = await postsApi.addComment(postId, payload);
        setPosts((prev) =>
          prev.map((p) =>
            p.id === postId
              ? { ...p, comments: [newComment, ...(p.comments || [])] }
              : p
          )
        );
        return newComment;
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to add comment');
        throw err;
      }
    },
    []
  );

  return { posts, loading, error, refresh, create, delete: delete_, togglePin, addComment, setError };
}
