import { Component, inject, ViewChild } from '@angular/core';
import { ContentManagementService } from '../../../Service/ContentManagement/content-management.service';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { Filter } from '../../../Model/Filter/Filter';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { PropertyService } from '../../../Service/Product/property.service';
import { Router } from '@angular/router';
import { ConfirmDialogComponent } from '../../dialogs/confirm-dialog/confirm-dialog.component';
import { ErrorDialogComponent } from '../../dialogs/error-dialog/error-dialog.component';



@Component({
  selector: 'app-content-management',
  standalone: false,
  templateUrl: './content-management.component.html',
  styleUrl: './content-management.component.css'
})
export class ContentManagementComponent {
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  private destroyComponent$ = new Subject<void>();
  private searchInputSubject = new Subject<string>();
  pageSize = 5;
  currentPage = 0;



  curFilter: Filter | undefined;
  displayedColumns: string[] = ['ID', 'Position', 'Actions'];
  dataSource = new MatTableDataSource<Filter>();
  searchValue: string = '';
  readonly dialog = inject(MatDialog);


  constructor(private router: Router, private productPropertyService: PropertyService, private contenService: ContentManagementService) { }

  ngOnInit(): void {
    this.loadFilters();
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

  private loadFilters(): void {
    this.contenService.getPagedFilters(this.currentPage + 1, this.pageSize, this.searchValue)
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
            console.error('Error loading product properties:', error);
          }
        }
      );
  }

  DeleteFilter() {
    if (this.curFilter && this.curFilter.ID) {
      this.contenService.deleteFilter(this.curFilter.ID).subscribe({
        next: (response) => {
          this.loadFilters();
        },
        error: (error) => {
          this.openErrorDialog("300ms", "150ms", "Lỗi xóa sản phẩm", error.message)
          console.log(error);
        }
      })
    }
  }


  OnClickCreateFilter(): void {
    this.router.navigate(['product-form'], {
      queryParams: {
        typeProductForm: "Create",
      }
    });
  }

  OnClickUpdateFilteer(filter: Filter): void {
    this.router.navigate(['product-form'], {
      queryParams: {
        typeContentManagementForm: "Update",
        filterID: filter.ID
      }
    });
  }

  OnClickDeleteFilter(filter: Filter): void {
    this.curFilter = filter;
    this.openConfirmDialog("300ms", "150ms", "xóa filter");
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadFilters();
  }

  onSearchInput(event: Event): void {
    const inputValue = (event.target as HTMLInputElement).value;
    this.searchValue = inputValue; // Cập nhật biến searchKeyword ngay lập tức
    this.searchInputSubject.next(inputValue);
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.loadFilters();
  }

  onFilterChange(): void {
    this.currentPage = 0;
    this.loadFilters();
  }

  openConfirmDialog(enterAnimationDuration: string, exitAnimationDuration: string, actionName: string): void {
    this.dialog.open(ConfirmDialogComponent, {
      width: '300px', // Tăng width để dễ nhìn hơn
      enterAnimationDuration,
      exitAnimationDuration,
      // ✨ TRUYỀN PHƯƠNG THỨC VÀO ĐÂY QUA THUỘC TÍNH 'data' ✨
      data: {
        actionName: actionName,
        onConfirm: () => this.DeleteFilter()
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

  ngOnDestroy(): void {
    this.destroyComponent$.next();
    this.destroyComponent$.complete();
  }
}
