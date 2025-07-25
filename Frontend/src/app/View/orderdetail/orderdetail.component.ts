import { Component, Inject, Input } from '@angular/core';
import { CustomerInformation } from '../../Model/User/CustomerInformation';
import { OrderItem } from '../../Model/Order/OrderItem';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { UserService } from '../../Service/User/user.service';
import { Order } from '../../Model/Order/Order';
import { OrderService } from '../../Service/Order/order.service';

@Component({
  selector: 'app-orderdetail',
  standalone: false,
  templateUrl: './orderdetail.component.html',
  styleUrl: './orderdetail.component.css'
})
export class OrderdetailComponent {
displayedOrderItemsColumns: string[] = ['No', 'Image', 'ProductName', 'Quantity', 'UnitPrice', 'LineTotal'];

  customerInformation: CustomerInformation | null = null;
  orderItems : OrderItem [] = [];

  constructor(
    public dialogRef: MatDialogRef<OrderdetailComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Order,
   private orderService : OrderService
  ) { }


  ngOnInit() {
    this.loadOrderItems();
  }

  loadOrderItems()
  {
    this.orderService.getOrderItemsOfOrder(this.data.ID).subscribe({
      next: (response) => {
        this.orderItems = response;
      }, error: (error) => {
        console.log(error);
      }
    })
  }

    getStatusTranslation(status: string): string {
    switch (status) {
      case 'Pending':
        return 'Đang chờ xử lý';
      case 'Processing':
        return 'Đang xử lý';
      case 'Shipped':
        return 'Đã giao đi';
      case 'Delivered':
        return 'Đã giao hàng';
      case 'Cancelled':
        return 'Đã hủy';
      case 'Refunded':
        return 'Đã hoàn tiền';
      case 'Returned':
        return 'Đã trả hàng';
      case 'Completed':
        return 'Hoàn tất';
      default:
        return status; // Trả về nguyên gốc nếu không tìm thấy bản dịch
    }
  }

  onClose(): void {
    this.dialogRef.close(); // Đóng dialog
  }
}
