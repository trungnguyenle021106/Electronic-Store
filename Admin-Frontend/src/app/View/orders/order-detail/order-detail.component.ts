import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Order } from '../../../Model/Order/Order';
import { UserService } from '../../../Service/User/user.service';
import { CustomerInformation } from '../../../Model/User/CustomerInformation';
import { OrderService } from '../../../Service/Order/order.service';
import { OrderItem } from '../../../Model/Order/OrderItem';

@Component({
  selector: 'app-order-detail',
  standalone: false,
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.css'
})
export class OrderDetailComponent {
  displayedOrderItemsColumns: string[] = ['No', 'Image', 'ProductName', 'Quantity', 'UnitPrice', 'LineTotal'];

  customerInformation: CustomerInformation | null = null;
  orderItems : OrderItem [] = [];

  constructor(
    public dialogRef: MatDialogRef<OrderDetailComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Order,
    private userService: UserService, private orderService : OrderService
  ) { }


  ngOnInit() {
    this.loadCustomer();
    this.loadOrderItems();
  }

  loadCustomer() {
    this.userService.getCustomerInformationByCustomerID(this.data.CustomerID).subscribe({
      next: (response) => {
        this.customerInformation = response;
        console.log(response)
      }, error: (error) => {
        console.log(error);
      }
    })
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
