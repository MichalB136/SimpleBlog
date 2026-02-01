import { useState, useCallback, useEffect } from 'react';
import type { Product, CartItem as CartItemType } from '@/types/product';

export interface CartItem extends CartItemType {}

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

  const addItem = useCallback((product: Product, quantity: number = 1, selectedColor?: string) => {
    setItems(prevItems => {
      const existing = prevItems.find(item => item.id === product.id && item.selectedColor === selectedColor);
      if (existing) {
        // Increase quantity if already in cart (same color)
        return prevItems.map(item =>
          item.id === product.id && item.selectedColor === selectedColor
            ? { ...item, quantity: item.quantity + quantity }
            : item
        );
      }
      // Add new item to cart including selectedColor
      return [...prevItems, { ...product, quantity, selectedColor }];
    });
  }, []);

  const removeItem = useCallback((productId: string, selectedColor?: string) => {
    setItems(prevItems => prevItems.filter(item => !(item.id === productId && item.selectedColor === selectedColor)));
  }, []);

  const updateQuantity = useCallback((productId: string, quantity: number, selectedColor?: string) => {
    if (quantity <= 0) {
      removeItem(productId, selectedColor);
    } else {
      setItems(prevItems =>
        prevItems.map(item =>
          item.id === productId && item.selectedColor === selectedColor
            ? { ...item, quantity }
            : item
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
