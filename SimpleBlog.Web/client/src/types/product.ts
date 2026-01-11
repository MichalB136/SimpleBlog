export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  stock: number;
  imageUrl?: string;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  stock: number;
  imageUrl?: string;
}

export interface UpdateProductRequest {
  name: string;
  description: string;
  price: number;
  stock: number;
  imageUrl?: string;
}

export interface CartItem extends Product {
  quantity: number;
}

export interface Order {
  id: string;
  createdAt: string;
  items: CartItem[];
  total: number;
  status: string;
}

export interface CreateOrderRequest {
  items: Array<{ productId: string; quantity: number }>;
}
