import { Injectable } from '@angular/core';
import { Product } from '../../Model/Product/Product';
import { BehaviorSubject, Observable } from 'rxjs';

export interface CartItem {
  product: Product;
  quantity: number;
}

const CART_STORAGE_KEY = 'shopping_cart'; // Đây là hằng số bạn muốn sử dụng

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private _cartItems: CartItem[] = [];
  private _cartSubject: BehaviorSubject<CartItem[]>;

  constructor() {
    this._cartItems = this.loadCartFromLocalStorage();
    this._cartSubject = new BehaviorSubject<CartItem[]>(this._cartItems);
  }

  get cart$(): Observable<CartItem[]> {
    return this._cartSubject.asObservable();
  }

  getCart(): CartItem[] {
    return [...this._cartItems];
  }

  addToCart(product: Product, quantity: number = 1): void {
    const existingItem = this._cartItems.find(item => item.product.ID === product.ID);

    if (existingItem) {
      existingItem.quantity += quantity;
    } else {
      this._cartItems.push({ product, quantity });
    }
    this.saveCartToLocalStorage();
    this._cartSubject.next(this._cartItems);
  }

  updateQuantity(productId: number, quantity: number): void {
    const item = this._cartItems.find(item => item.product.ID === productId);
    if (item) {
      if (quantity <= 0) {
        this.removeFromCart(productId);
      } else {
        item.quantity = quantity;
        this.saveCartToLocalStorage();
        this._cartSubject.next(this._cartItems);
      }
    }
  }

  removeFromCart(productId: number): void {
    this._cartItems = this._cartItems.filter(item => item.product.ID !== productId);
    this.saveCartToLocalStorage();
    this._cartSubject.next(this._cartItems);
  }

  clearCart(): void {
    this._cartItems = [];
    localStorage.removeItem(CART_STORAGE_KEY); // Sử dụng biến CART_STORAGE_KEY ở đây
    this._cartSubject.next(this._cartItems);
  }

  getTotalItems(): number {
    return this._cartItems.reduce((total, item) => total + item.quantity, 0);
  }

  getTotalPrice(): number {
    return this._cartItems.reduce((total, item) => total + (item.product.Price * item.quantity), 0);
  }

  private saveCartToLocalStorage(): void {
    try {
      localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(this._cartItems)); // Đã sửa
    } catch (e) {
      console.error('Lỗi khi lưu giỏ hàng vào Local Storage', e);
    }
  }

  private loadCartFromLocalStorage(): CartItem[] {
    try {
      const storedCart = localStorage.getItem(CART_STORAGE_KEY); // Đã sửa
      return storedCart ? JSON.parse(storedCart) : [];
    } catch (e) {
      console.error('Lỗi khi tải giỏ hàng từ Local Storage', e);
      return [];
    }
  }
}