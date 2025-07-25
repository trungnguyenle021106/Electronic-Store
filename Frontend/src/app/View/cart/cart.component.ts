import { Component, inject } from '@angular/core';
import { Observable, Subject, takeUntil } from 'rxjs';
import { CartItem, CartService } from '../../Service/Cart/cart.service';
import { Product } from '../../Model/Product/Product';
import { AuthService } from '../../Service/Auth/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { OrderService } from '../../Service/Order/order.service';
import { OrderDetail } from '../../Model/Order/OrderDetailt';
import { Order } from '../../Model/Order/Order';
import { OrderdetailComponent } from '../orderdetail/orderdetail.component';
import { MatDialog } from '@angular/material/dialog';

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
  isLoading: boolean = false;
  isLoggedIn: Observable<boolean>;
  userRole: Observable<string | null>;

  readonly dialog = inject(MatDialog);

  order: Order | null = null;
  private _userRoleStatus: string | null = null;
  private _isLoggedInStatus: boolean = false;
  private destroy$ = new Subject<void>();
  constructor(public cartService: CartService, private authService: AuthService, private snackBar: MatSnackBar, private orderService: OrderService) {
    this.isLoggedIn = this.authService.isLoggedIn;
    this.userRole = this.authService.currentUserRole;
  }

  ngOnInit(): void {
    this.cartItems$ = this.cartService.cart$;

    this.userRole
      .pipe(takeUntil(this.destroy$)) // Tự động hủy đăng ký khi component bị hủy
      .subscribe({
        next: (response) => {
          this._userRoleStatus = response; // Cập nhật biến trạng thái cục bộ
        }
      });

    this.isLoggedIn
      .pipe(takeUntil(this.destroy$)) // Tự động hủy đăng ký khi component bị hủy
      .subscribe({
        next: (response) => {
          this._isLoggedInStatus = response; // Cập nhật biến trạng thái cục bộ
        }
      });
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
    this.cartService.clearCart();
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }


  OnclickDetail(): void {

    // Mở dialog và truyền dữ liệu đơn hàng vào
    const dialogRef = this.dialog.open(OrderdetailComponent, {
      width: '800px', // Đặt độ rộng của dialog
      data: this.order // Truyền đối tượng order vào dialog
    });

    // Tùy chọn: Xử lý kết quả khi dialog đóng
    dialogRef.afterClosed().subscribe(result => {
      console.log('Dialog đã đóng với kết quả:', result);
      // Bạn có thể làm gì đó với `result` nếu dialog trả về dữ liệu
    });
  }


  OnClickNextStep() {
    if (this.step === 'cart' && this._isLoggedInStatus) {
      this.step = 'order-info'
    } else if (this.step === 'cart' && !this._isLoggedInStatus) {
      this.snackBar.open('Hãy đăng nhập để đặt hàng', 'Đóng', {
        duration: 3000,
        horizontalPosition: 'end',  // Bên phải
        verticalPosition: 'top',    // Phía trên
        panelClass: ['success-snackbar']
      });
    } else if (this.step === 'order-info' && this._userRoleStatus == "Customer") {

      let orderDetails: OrderDetail[] = [];

      this.cartService.getCart().forEach(cartItem => {
        let orderDetail: OrderDetail = {
          ProductID: cartItem.product.ID,
          Quantity: cartItem.quantity,
          TotalPrice: cartItem.product.Price * cartItem.quantity
        }
        orderDetails.push(orderDetail);
      });
      this.isLoading = true;
      this.orderService.createOrder(orderDetails).subscribe({
        next: (response) => {
          this.order = response;
          this.step = 'complete'
          this.isLoading = false;
          this.cartService.clearCart();
        },
        error: (error) => {
          this.isLoading = false;
          this.snackBar.open('Có lỗi đã xảy ra, vui lòng thử lại', 'Đóng', {
            duration: 3000,
            horizontalPosition: 'end',  // Bên phải
            verticalPosition: 'top',    // Phía trên
            panelClass: ['success-snackbar']
          });
        }
      });
    }
    else if (this._userRoleStatus == "Admin") {
      this.snackBar.open('Sử dụng tài khoản khách hàng để sử dụng tính năng này', 'Đóng', {
        duration: 3000,
        horizontalPosition: 'end',  // Bên phải
        verticalPosition: 'top',    // Phía trên
        panelClass: ['success-snackbar']
      });
    }
  }

  OnClickBackStep() {
    if (this.step == 'order-info') {
      this.step = 'cart'
    }
  }

  ngOnDestroy() {
    this.destroy$.next(); // Phát ra tín hiệu
    this.destroy$.complete(); // Hoàn thành Subject để giải phóng tài nguyên
  }
}
