import { Component, inject, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { debounceTime, distinctUntilChanged, map, Subject, switchMap, takeUntil } from 'rxjs';
import { Order } from '../../../Model/Order/Order';
import { MatTableDataSource } from '@angular/material/table';
import { OrderService } from '../../../Service/Order/order.service';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../dialogs/confirm-dialog/confirm-dialog.component';
import { ErrorDialogComponent } from '../../dialogs/error-dialog/error-dialog.component';
import { SignalRService } from '../../../Service/SignalR/signal-r.service';
import { OrderDetailComponent } from '../order-detail/order-detail.component';
import { OrderItem } from '../../../Model/Order/OrderItem';
import { AnalyticsService } from '../../../Service/Analytic/analytics.service';
import { AnalyzeOrderRequest } from '../../../Model/Analytic/AnalyzeOrderRequest';
import { ProductStatistics } from '../../../Model/Analytic/ProductStatistics';

@Component({
  selector: 'app-order',
  standalone: false,
  templateUrl: './order.component.html',
  styleUrl: './order.component.css'
})
export class OrderComponent {
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  private destroyComponent$ = new Subject<void>();
  private searchInputSubject = new Subject<string>();
  pageSize = 5;
  currentPage = 0;


  curOrder: Order | undefined;
  displayedColumns: string[] = ['ID', 'CustomerID', 'OrderDate', 'Total', 'Status', 'Actions'];
  dataSource = new MatTableDataSource<Order>();
  tempOrders: Order[] = [];
  searchValue: string = '';

  orderItems: OrderItem[] = [];
  orderId: number = 0;
  order: Order | null = null;
  status: string = "";

  totalCount: number = 0;

  private orderHubUrl = 'http://localhost:5293/gateway/order-apis/orderHub';
  readonly dialog = inject(MatDialog);


  constructor(private router: Router, private orderService: OrderService, private signalrService: SignalRService, private analyticService: AnalyticsService) { }

  ngOnInit(): void {
    this.loadOrders();
    this.searchInputSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroyComponent$)
      )
      .subscribe(() => {
        this.onSearchChange();
      });

    this.connectSignalR();
  }

  ngAfterViewInit(): void {
    this.paginator.page
      .pipe(takeUntil(this.destroyComponent$))
      .subscribe((event: PageEvent) => {
        this.onPageChange(event);
      });
  }



  private loadOrders(): void {
    this.orderService.getPagedOrders(this.currentPage + 1, this.pageSize, this.searchValue)
      .pipe(takeUntil(this.destroyComponent$)) // Hủy đăng ký khi component bị hủy
      .subscribe(
        {
          next: (response) => {
            this.dataSource.data = response.Items; // Cập nhật dữ liệu cho MatTable
            this.tempOrders = JSON.parse(JSON.stringify(response.Items));
            if (this.paginator) {
              this.totalCount = response.TotalCount;
              this.paginator.length = response.TotalCount; // Đảm bảo MatPaginator nhận giá trị totalProducts
              this.paginator.pageIndex = this.currentPage; // Đảm bảo MatPaginator hiển thị đúng trang\
            }
          },
          error: (error) => {
            console.error('Error loading orders:', error);
          }
        }
      );
  }

  OnclickDetail(order: Order) {
    console.log('Xem chi tiết đơn hàng:', order);

    // Mở dialog và truyền dữ liệu đơn hàng vào
    const dialogRef = this.dialog.open(OrderDetailComponent, {
      width: '800px', // Đặt độ rộng của dialog
      data: order // Truyền đối tượng order vào dialog
    });

    // Tùy chọn: Xử lý kết quả khi dialog đóng
    dialogRef.afterClosed().subscribe(result => {
      console.log('Dialog đã đóng với kết quả:', result);
      // Bạn có thể làm gì đó với `result` nếu dialog trả về dữ liệu
    });
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadOrders();
  }

  onSearchInput(event: Event): void {
    const inputValue = (event.target as HTMLInputElement).value;
    this.searchValue = inputValue; // Cập nhật biến searchKeyword ngay lập tức
    this.searchInputSubject.next(inputValue);
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.loadOrders();
  }

  onFilterChange(): void {
    this.currentPage = 0;
    this.loadOrders();
  }

  openConfirmDialog(enterAnimationDuration: string, exitAnimationDuration: string, actionName: string): void {
    // Mở dialog và lưu đối tượng tham chiếu (MatDialogRef)
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '300px', // Tăng width để dễ nhìn hơn
      enterAnimationDuration,
      exitAnimationDuration,
      data: {
        actionName: actionName,
      }
    });

    // Bắt sự kiện khi dialog đóng lại
    dialogRef.afterClosed().subscribe(result => {
      // Biến `result` sẽ là giá trị được truyền khi dialog đóng
      // Ví dụ: `true` nếu xác nhận, `false` hoặc `undefined` nếu hủy
      if (result) {
        this.UpdateOrderStatus();
      } else {
        this.dataSource.data = this.tempOrders; // Cập nhật dữ liệu cho MatTable
        if (this.paginator) {
          this.paginator.length = this.totalCount; // Đảm bảo MatPaginator nhận giá trị totalProducts
          this.paginator.pageIndex = this.currentPage; // Đảm bảo MatPaginator hiển thị đúng trang\
        }
      }
    });
  }

  UpdateOrderStatus() {
    this.orderService.updateOrder(this.orderId, this.status).subscribe({
      next: (response) => {
        this.orderId = response.ID;
        this.order = response;

        this.AnalyzeOrderByDate();
        this.AnalyzeProductStatistics();
        this.loadOrders();
      },
      error: (error) => {
        console.error('Lỗi khi cập nhật trạng thái đơn hàng:', error);
        this.openErrorDialog("300ms", "150ms", "Lỗi cập nhật đơn hàng", error.message)
      }
    });
  }

  AnalyzeOrderByDate() {
    if (this.order) {
      let CancelledOrders: number = 0;
      if (this.order?.Status == "Đã hủy") {
        CancelledOrders = 1;
      }
      const body: AnalyzeOrderRequest = {
        Date: this.order?.OrderDate,
        Total: this.order.Total,
        CancelledOrders: CancelledOrders
      }
      this.analyticService.analyzeOrderByDate(body).subscribe({
        next: (response) => {

        },
        error: (error) => {
          console.error('Lỗi khi thêm dữ liệu thống kê:', error);
        }
      })
    }
  }


  AnalyzeProductStatistics(): void {
    if (this.order) {
      this.orderService.getOrderItemsOfOrder(this.orderId).pipe(
        // Bước 1: Sử dụng map để chuyển đổi dữ liệu orderItems
        map((orderItems: OrderItem[]) => {
          // Xử lý dữ liệu và tạo body cho request tiếp theo
          const body: ProductStatistics[] = orderItems.map(item => ({
            ProductID: item.Product.ID,
            TotalSales: item.Quantity
          }));
          return body;
        }),
        // Bước 2: Sử dụng switchMap để chuyển sang Observable mới
        switchMap((body: ProductStatistics[]) => {
          // Lời gọi API thứ hai được trả về ở đây
          return this.analyticService.analyzeProductStatistics(body);
        })
      ).subscribe({
        next: (response) => {
          // Xử lý kết quả của lời gọi API thứ hai (analyzeProductStatistics)
          console.log('Thêm dữ liệu thống kê thành công:', response);
        },
        error: (error) => {
          // Xử lý tất cả lỗi từ cả hai lời gọi API ở đây
          console.error('Lỗi trong quá trình xử lý thống kê:', error);
        }
      });
    }
  }


  openErrorDialog(enterAnimationDuration: string, exitAnimationDuration: string, errorTitle: string, errorMessage: string): void {
    this.dialog.open(ErrorDialogComponent, {
      width: '300px', // Kích thước phù hợp với dialog lỗi
      enterAnimationDuration,
      exitAnimationDuration,
      // Truyền tiêu đề và thông báo lỗi vào dialog
      data: {
        title: errorTitle,
        message: errorMessage
      },
      disableClose: true, // Thường là lỗi thì không cho click ra ngoài đóng
      hasBackdrop: true, // Luôn có backdrop
    });
  }

  onStatusChange(orderId: number, newStatus: string): void {
    this.orderId = orderId;
    this.status = newStatus;

    this.openConfirmDialog("300ms", "150ms", "cập nhật trạng thái đơn hàng");
  }

  private connectSignalR() {
    this.signalrService.on<any[]>(this.orderHubUrl, 'OrderChanged')
      .pipe(takeUntil(this.destroyComponent$))
      .subscribe(
        response => {
          if (Array.isArray(response) && response.length >= 2) {
            const message = response[1]; // Lấy phần tử thứ hai của mảng

            if (message === 'OrderAdded') {
              this.loadOrders(); // Tải lại trang hiện tại
            } else if (message === 'OrderUpdated') { // Xử lý nếu có event update
              this.loadOrders();
            }
          } else {
            console.warn('SignalR: Unexpected response format:', response);
          }
        },
        error => {
          console.log(error)
        });
  }

  ngOnDestroy(): void {
    this.destroyComponent$.next();
    this.destroyComponent$.complete();
  }
}
