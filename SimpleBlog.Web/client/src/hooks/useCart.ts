import { useState, useCallback, useEffect } from 'react';
import type { Product } from '@/types/product';

export interface CartItem extends Product {
  quantity: number;
}

export function useCart() {
  const [items, setItems] = useState<CartItem[]>(() => {
    try {
      const saved = localStorage.getItem('cart');
      return saved ? JSON.parse(saved) : [];
    } catch {
      return [];
    }
  });

  const [totalPrice, setTotalPrice] = useState(() =>
    items.reduce((sum, item) => sum + item.price * item.quantity, 0)
  );

  // Persist cart to localStorage and keep totalPrice in sync when items change
  useEffect(() => {
    try {
      localStorage.setItem('cart', JSON.stringify(items));
    } catch {
      // ignore storage errors
    }
    const total = items.reduce((sum, item) => sum + item.price * item.quantity, 0);
    setTotalPrice(total);
  }, [items]);

  const addItem = useCallback((product: Product, quantity: number = 1) => {
    setItems(prevItems => {
      const existing = prevItems.find(item => item.id === product.id);
      if (existing) {
        // Increase quantity if already in cart
        return prevItems.map(item =>
          item.id === product.id
            ? { ...item, quantity: item.quantity + quantity }
            : item
        );
      }
      // Add new item to cart
      return [...prevItems, { ...product, quantity }];
    });
  }, []);

  const removeItem = useCallback((productId: string) => {
    setItems(prevItems => prevItems.filter(item => item.id !== productId));
  }, []);

  const updateQuantity = useCallback((productId: string, quantity: number) => {
    if (quantity <= 0) {
      removeItem(productId);
    } else {
      setItems(prevItems =>
        prevItems.map(item =>
          item.id === productId ? { ...item, quantity } : item
        )
      );
    }
  }, [removeItem]);

  const clearCart = useCallback(() => {
    setItems([]);
  }, []);

  const itemCount = items.reduce((sum, item) => sum + item.quantity, 0);

  return {
    items,
    itemCount,
    totalPrice,
    addItem,
    removeItem,
    updateQuantity,
    clearCart,
  };
}
