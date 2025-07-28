import { Component, inject, ViewChild } from '@angular/core';
import { UserService } from '../../../Service/User/user.service';
import { MatDialog } from '@angular/material/dialog';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { Account } from '../../../Model/User/Account';
import { MatTableDataSource } from '@angular/material/table';
import { CustomerComponent } from '../customer/customer.component';
import { ConfirmDialogComponent } from '../../dialogs/confirm-dialog/confirm-dialog.component';
import { Router } from '@angular/router';
import { ErrorDialogComponent } from '../../dialogs/error-dialog/error-dialog.component';

@Component({
  selector: 'app-user',
  standalone: false,
  templateUrl: './user.component.html',
  styleUrl: './user.component.css'
})
export class UserComponent {
 @ViewChild(MatPaginator) paginator!: MatPaginator;

  private destroyComponent$ = new Subject<void>();
  private searchInputSubject = new Subject<string>();
  pageSize = 5;
  currentPage = 0;


  curAccount: Account | undefined;
  displayedColumns: string[] = ['ID', 'Email', 'Role', 'Status', 'Actions'];
  dataSource = new MatTableDataSource<Account>();
  searchValue: string = '';

  accountId: number = 0;
  status: string = "";

  readonly dialog = inject(MatDialog);


  constructor( private userService: UserService) { }

  ngOnInit(): void {
    this.loadUsers();
    this.searchInputSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroyComponent$)
      )
      .subscribe(() => {
        this.onSearchChange();
      });
  }

  ngAfterViewInit(): void {
    this.paginator.page
      .pipe(takeUntil(this.destroyComponent$))
      .subscribe((event: PageEvent) => {
        this.onPageChange(event);
      });
  }


  private loadUsers(): void {
    this.userService.getPagedAccounts(this.currentPage + 1, this.pageSize, this.searchValue)
      .pipe(takeUntil(this.destroyComponent$)) // Hủy đăng ký khi component bị hủy
      .subscribe(
        {
          next: (response) => {
            this.dataSource.data = response.Items; // Cập nhật dữ liệu cho MatTable
            if (this.paginator) {
              this.paginator.length = response.TotalCount; // Đảm bảo MatPaginator nhận giá trị totalProducts
              this.paginator.pageIndex = this.currentPage; // Đảm bảo MatPaginator hiển thị đúng trang\
            }
          },
          error: (error) => {
            console.error('Error loading accounts:', error);
          }
        }
      );
  }

  OnclickDetail(account: Account) {

    // Mở dialog và truyền dữ liệu đơn hàng vào
    const dialogRef = this.dialog.open(CustomerComponent, {
      width: '800px', // Đặt độ rộng của dialog
      data: account.ID // Truyền đối tượng order vào dialog
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
    this.loadUsers();
  }

  onSearchInput(event: Event): void {
    const inputValue = (event.target as HTMLInputElement).value;
    this.searchValue = inputValue; // Cập nhật biến searchKeyword ngay lập tức
    this.searchInputSubject.next(inputValue);
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.loadUsers();
  }

  onFilterChange(): void {
    this.currentPage = 0;
    this.loadUsers();
  }

  openConfirmDialog(enterAnimationDuration: string, exitAnimationDuration: string, actionName: string): void {
    this.dialog.open(ConfirmDialogComponent, {
      width: '300px', // Tăng width để dễ nhìn hơn
      enterAnimationDuration,
      exitAnimationDuration,
      // ✨ TRUYỀN PHƯƠNG THỨC VÀO ĐÂY QUA THUỘC TÍNH 'data' ✨
      data: {
        actionName: actionName,
        onConfirm: () => this.UpdateAccountStatus()
      }
    });
  }

  UpdateAccountStatus() {
    this.userService.updateAccount(this.accountId, this.status).subscribe({
      next: (response) => {
        this.loadUsers();
      },
      error: (error) => {
        console.error('Lỗi khi cập nhật trạng thái tài khoản:', error);
        this.openErrorDialog("300ms", "150ms", "Lỗi cập nhật tài khoản", error.message)
      }
    });
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
    this.accountId = orderId;
    this.status = newStatus;

    this.openConfirmDialog("300ms", "150ms", "cập nhật trạng thái tài khoản");
  }

  ngOnDestroy(): void {
    this.destroyComponent$.next();
    this.destroyComponent$.complete();
  }
}
