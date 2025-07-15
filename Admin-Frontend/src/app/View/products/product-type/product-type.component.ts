import { Component, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { ProductType } from '../../../Model/Product/ProductType';
import { MatTableDataSource } from '@angular/material/table';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { ProductTypeService } from '../../../Service/Product/product-type.service';
import { SignalRService } from '../../../Service/SignalR/signal-r.service';

@Component({
  selector: 'app-product-type',
  standalone: false,
  templateUrl: './product-type.component.html',
  styleUrl: './product-type.component.css'
})
export class ProductTypeComponent {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  typePropertyForm: string = '';
  curProductType: ProductType | undefined;

  searchKeyword: string = '';
  filterPropertyName: string = '';

  totalProducts = 0;
  pageSize = 10;
  currentPage = 0;

  isConFirmDisplayed: boolean = false;
  isFormPropertyDisplayed: boolean = false;
  isOverlayDisplayed: boolean = false;
  displayedColumns: string[] = ['ID', 'TÊN', 'HÀNH ĐỘNG'];

  dataSource = new MatTableDataSource<ProductType>();

  private searchInputSubject = new Subject<string>();

  private productNotificationHubUrl = 'http://localhost:5293/gateway/product-apis/productTypeHub';
  private destroyComponent$ = new Subject<void>();

  constructor(private ProductTypeService: ProductTypeService, private signalrService: SignalRService) {


  }

  ngOnInit(): void {
    this.loadProductTypes();
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
    // Gán paginator và sort cho dataSource sau khi view được khởi tạo 
    this.dataSource.sort = this.sort;

    // Đăng ký lắng nghe sự kiện thay đổi trang của MatPaginator
    // Đây là cách đúng để bắt sự kiện thay đổi trang từ component MatPaginator
    this.paginator.page
      .pipe(takeUntil(this.destroyComponent$))
      .subscribe((event: PageEvent) => {
        this.onPageChange(event);
      });
  }

  private connectSignalR() {
    this.signalrService.on<any[]>(this.productNotificationHubUrl, 'ProductTypeChanged')
      .pipe(takeUntil(this.destroyComponent$))
      .subscribe(
        response => {
          if (Array.isArray(response) && response.length >= 2) {
            const message = response[1]; // Lấy phần tử thứ hai của mảng

            if (message === 'ProductTypeAdded') {
              this.loadProductTypes(); // Tải lại trang hiện tại
            } else if (message === 'ProductTypeUpdated') { // Xử lý nếu có event update
              this.loadProductTypes();
            } else if (message === 'ProductTypeDeleted') { // Xử lý nếu có event delete
              this.loadProductTypes();
            }
          } else {
            console.warn('SignalR: Unexpected response format:', response);
          }
        },
        error => {
          console.log(error)
        });
  }

  private loadProductTypes(): void {
    this.ProductTypeService.getPagedProductTypes(this.currentPage + 1, this.pageSize, this.searchKeyword, this.filterPropertyName)
      .pipe(takeUntil(this.destroyComponent$)) // Hủy đăng ký khi component bị hủy
      .subscribe(
        {
          next: (response) => {
            this.dataSource.data = response.Items; // Cập nhật dữ liệu cho MatTable
            this.totalProducts = response.TotalCount; // Cập nhật tổng số lượng cho MatPaginator
            if (this.paginator) {
              this.paginator.length = this.totalProducts; // Đảm bảo MatPaginator nhận giá trị totalProducts
              this.paginator.pageIndex = this.currentPage; // Đảm bảo MatPaginator hiển thị đúng trang\
            }
          },
          error: (error) => {
            console.error('Error loading product brands:', error);
          }
        }
      );
  }

  onPageChange(event: PageEvent): void {

    this.currentPage = event.pageIndex;
    console.log(this.currentPage)
    this.pageSize = event.pageSize;
    this.loadProductTypes();
  }

  onSearchChange(): void {
    this.currentPage = 0; // Reset về trang đầu tiên
    this.loadProductTypes();
  }

  onFilterChange(): void {
    this.currentPage = 0; // Reset về trang đầu tiên khi lọc mới
    this.loadProductTypes();
  }

  onSearchInput(event: Event): void {
    const inputValue = (event.target as HTMLInputElement).value;
    this.searchKeyword = inputValue; // Cập nhật biến searchKeyword ngay lập tức
    this.searchInputSubject.next(inputValue);
  }

  DeleteProductType() {
    if (this.curProductType) {
      this.ProductTypeService.deleteProductType(this.curProductType.ID).subscribe({
        next: (response) => {
          this.DisplayConfirmForm(false);
          this.DisplayOverlay(false);
        },
        error: (error) => {
          alert(error.message);
          console.log(error);
        }
      })
    }
  }

  OnclickDelete(productType: ProductType) {
    this.DisplayOverlay(true);
    this.DisplayConfirmForm(true);
    this.curProductType = productType;
  }

  OnclickUpdate(productType: ProductType) {
    this.curProductType = productType;
    this.DisplayOverlay(true);
    this.SetTypePropertyForm("Update");
    this.DisplayFormProperty(true);
  }

  DisplayOverlay(isDisplayed: boolean): void {
    this.isOverlayDisplayed = isDisplayed;
  }

  SetTypePropertyForm(typePropertyForm: string): void {
    this.typePropertyForm = typePropertyForm;
  }

  DisplayFormProperty(isDisplayed: boolean): void {
    this.isFormPropertyDisplayed = isDisplayed;
  }

  DisplayConfirmForm(isShow: boolean): void {
    this.isConFirmDisplayed = isShow;
  }

  OnAcceptDelete() {
    this.DeleteProductType();
  }

  ngOnDestroy(): void {
    this.destroyComponent$.next();
    this.destroyComponent$.complete();
    this.signalrService.stopAllConnections();
  }
}
