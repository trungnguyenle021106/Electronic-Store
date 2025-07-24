import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { CartItem, CartService } from '../../Service/Cart/cart.service';
import { Product } from '../../Model/Product/Product';

@Component({
  selector: 'app-cart',
  standalone: false,
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class CartComponent {
  cartItems$!: Observable<CartItem[]>;
  sampleProduct: Product = { // Dữ liệu sản phẩm mẫu để test
    ID: 12345,
    Name: 'Laptop gaming ASUS ROG Zephyrus G16 GU605CM QR078W',
    Quantity: 10, // Số lượng trong kho
    Image: 'https://anphat.com.vn/media/product/51645_laptop_asus_rog_strix_g16_g614ju_n3480w__2_.jpg',
    ProductBrandID: 1,
    ProductTypeID: 1,
    Description: 'Laptop gaming mạnh mẽ với hiệu năng cao.',
    Price: 62990000,
    Status: 'In Stock'
  };

  step: string = "cart"
  constructor(public cartService: CartService) { }

  ngOnInit(): void {
    this.cartItems$ = this.cartService.cart$;

    // Thêm sản phẩm mẫu vào giỏ hàng để dễ dàng kiểm tra
    // Bạn có thể bỏ đoạn này khi đã có chức năng thêm sản phẩm từ trang khác
    // this.cartService.addToCart(this.sampleProduct, 1);
  }

  onRemoveItem(productId: number): void {
    this.cartService.removeFromCart(productId);
  }

  onUpdateQuantity(productId: number, currentQuantity: number, action: 'add' | 'remove'): void {
    let newQuantity = currentQuantity;
    if (action === 'add') {
      newQuantity++;
    } else if (action === 'remove') {
      newQuantity--;
    }
    this.cartService.updateQuantity(productId, newQuantity);
  }

  onClearCart(): void {
    if (confirm('Bạn có chắc muốn xóa toàn bộ giỏ hàng?')) {
      this.cartService.clearCart();
    }
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }

  OnClickNextStep() {
    if(this.step === 'cart')
    {
      this.step = 'order-info'
    }
  }

  OnClickBackStep()
  {
    if(this.step == 'order-info')
    {
      this.step = 'cart'
    }
  }
}
