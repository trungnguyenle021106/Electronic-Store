import { Component, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { ProductBrand } from '../../../Model/Product/ProductBrand';
import { MatTableDataSource } from '@angular/material/table';
import { SignalRService } from '../../../Service/SignalR/signal-r.service';
import { ProductBrandService } from '../../../Service/Product/product-brand.service';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-product-brand',
  standalone: false,
  templateUrl: './product-brand.component.html',
  styleUrl: './product-brand.component.css'
})
export class ProductBrandComponent {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  typePropertyForm: string = '';
  curProductBrand: ProductBrand | undefined;

  searchKeyword: string = '';
  filterPropertyName: string = '';

  totalProducts = 0;
  pageSize = 10;
  currentPage = 0;

  isConFirmDisplayed: boolean = false;
  isFormPropertyDisplayed: boolean = false;
  isOverlayDisplayed: boolean = false;
  displayedColumns: string[] = ['ID', 'TÊN', 'HÀNH ĐỘNG'];

  dataSource = new MatTableDataSource<ProductBrand>();

  private searchInputSubject = new Subject<string>();

  private productNotificationHubUrl = 'http://localhost:5293/gateway/product-apis/productBrandHub';
  private destroyComponent$ = new Subject<void>();

  constructor(private productBrandService: ProductBrandService, private signalrService: SignalRService) {


  }

  ngOnInit(): void {
    this.loadProductBrands();
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
    this.signalrService.on<any[]>(this.productNotificationHubUrl, 'ProductBrandChanged')
      .pipe(takeUntil(this.destroyComponent$))
      .subscribe(
        response => {
          if (Array.isArray(response) && response.length >= 2) {
            const message = response[1]; // Lấy phần tử thứ hai của mảng

            if (message === 'ProductBrandAdded') {
              this.loadProductBrands(); // Tải lại trang hiện tại
            } else if (message === 'ProductBrandUpdated') { // Xử lý nếu có event update
              this.loadProductBrands();
            } else if (message === 'ProductBrandDeleted') { // Xử lý nếu có event delete
              this.loadProductBrands();
            }
          } else {
            console.warn('SignalR: Unexpected response format:', response);
          }
        },
        error => {
          console.log(error)
        });
  }

  private loadProductBrands(): void {
    this.productBrandService.getPagedProductBrands(this.currentPage + 1, this.pageSize, this.searchKeyword, this.filterPropertyName)
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
    this.loadProductBrands();
  }

  onSearchChange(): void {
    this.currentPage = 0; // Reset về trang đầu tiên
    this.loadProductBrands();
  }

  onFilterChange(): void {
    this.currentPage = 0; // Reset về trang đầu tiên khi lọc mới
    this.loadProductBrands();
  }

  onSearchInput(event: Event): void {
    const inputValue = (event.target as HTMLInputElement).value;
    this.searchKeyword = inputValue; // Cập nhật biến searchKeyword ngay lập tức
    this.searchInputSubject.next(inputValue);
  }

  DeleteProductBrand() {
    if (this.curProductBrand) {
      this.productBrandService.deleteProductBrand(this.curProductBrand.ID).subscribe({
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

  OnclickDelete(productBrand: ProductBrand) {
    this.DisplayOverlay(true);
    this.DisplayConfirmForm(true);
    this.curProductBrand = productBrand;
  }

  OnclickUpdate(productBrand: ProductBrand) {
    this.curProductBrand = productBrand;
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
    this.DeleteProductBrand();
  }

  ngOnDestroy(): void {
    this.destroyComponent$.next();
    this.destroyComponent$.complete();
    this.signalrService.stopAllConnections();
  }
}
