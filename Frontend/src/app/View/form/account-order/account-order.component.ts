import { Component, EventEmitter, inject, Output } from '@angular/core';
import { Order } from '../../../Model/Order/Order';
import { OrderService } from '../../../Service/Order/order.service';
import { MatDialog } from '@angular/material/dialog';
import { OrderdetailComponent } from '../../orderdetail/orderdetail.component';

@Component({
  selector: 'app-account-order',
  standalone: false,
  templateUrl: './account-order.component.html',
  styleUrl: './account-order.component.css'
})
export class AccountOrderComponent {
  isEmptyOrder: boolean = false;
  selectedStatus: string = 'TẤT CẢ';

  searchKeyword: string = ''; // 💡 Biến để lưu từ khóa tìm kiếm
  orders: Order[] = [];
  allOrders: Order[] = [];
  readonly dialog = inject(MatDialog);
  statusMap: { [key: string]: string | undefined } = {
    'TẤT CẢ': undefined, // 'TẤT CẢ' không gửi tham số status lên API
    'Đang chờ xử lý': 'Đang chờ xử lý', // MỚI
    'Đang xử lý': 'Đang xử lý', // Trạng thái mới
    'Đã giao đi': 'Đã giao đi', // ĐANG VẬN CHUYỂN
    'Đã giao hàng': 'Đã giao hàng', // Nếu bạn muốn có tab riêng cho "Đã giao hàng"
    'Đã hủy': 'Đã hủy', // HUỶ
    'Đã hoàn tiền': 'Đã hoàn tiền',
    'Hoàn thành': 'Hoàn thành', // HOÀN THÀNH
  };


  constructor(private orderService: OrderService) {
    this.loadOrders(this.selectedStatus);
  }

  OnclickDetail(order: Order): void {
    console.log('Xem chi tiết đơn hàng:', order);

    // Mở dialog và truyền dữ liệu đơn hàng vào
    const dialogRef = this.dialog.open(OrderdetailComponent, {
      width: '800px', // Đặt độ rộng của dialog
      data: order // Truyền đối tượng order vào dialog
    });

    // Tùy chọn: Xử lý kết quả khi dialog đóng
    dialogRef.afterClosed().subscribe(result => {
      console.log('Dialog đã đóng với kết quả:', result);
      // Bạn có thể làm gì đó với `result` nếu dialog trả về dữ liệu
    });
  }

  applySearch(): void {
    const keyword = this.searchKeyword.toLowerCase().trim();

    if (!keyword) {
      // Nếu không có từ khóa, hiển thị tất cả đơn hàng gốc của trạng thái hiện tại
      this.orders = [...this.allOrders]; // Sao chép mảng gốc
    } else {
      // Lọc đơn hàng theo Mã đơn hàng (ID)
      this.orders = this.allOrders.filter(order =>
        order.ID?.toString().toLowerCase().includes(keyword)
        // Bạn có thể thêm các trường khác để tìm kiếm ở đây, ví dụ:
        // || order.Name?.toLowerCase().includes(keyword)
      );
    }
    // Cập nhật trạng thái rỗng
    this.isEmptyOrder = this.orders.length === 0;
    console.log(`Đã lọc ${this.orders.length} đơn hàng với từ khóa: "${keyword}"`);
  }

  selectStatus(statusKey: string): void {
    this.selectedStatus = statusKey;
    this.loadOrders(statusKey);
  }

  loadOrders(statusKey: string): void {
    // Đảm bảo có customer ID trước khi gọi API

    // Lấy giá trị status thực tế để gửi lên API từ statusMap
    const apiStatus = this.statusMap[statusKey];

    // Gọi API getOrderByCustomerID
    this.orderService.getOrderCurrentCustomer(apiStatus).subscribe({
      next: (orders: Order[]) => {
        this.allOrders = orders || [];
        // Sau đó áp dụng tìm kiếm ngay lập tức (nếu có từ khóa cũ)
        this.applySearch();

        this.isEmptyOrder = this.orders.length === 0;
        console.log(`Đã tải ${this.orders.length} đơn hàng cho trạng thái: ${statusKey}`);
        console.log(this.orders);
      },
      error: (error) => {
        console.error(`Lỗi khi tải đơn hàng cho trạng thái ${statusKey}:`, error);
        this.orders = []; // Xóa danh sách nếu có lỗi
        this.isEmptyOrder = true;
        // Hiển thị thông báo lỗi cho người dùng (tùy chọn)
      }
    });
  }
}
