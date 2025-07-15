import { Component, ViewChild } from '@angular/core';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { ProductProperty } from '../../../Model/Product/ProductProperty';
import { PropertyService } from '../../../Service/Product/property.service';
import { SignalRService } from '../../../Service/SignalR/signal-r.service';


@Component({
  selector: 'app-property',
  standalone: false,
  templateUrl: './property.component.html',
  styleUrl: './property.component.css',
})
export class PropertyComponent {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  typePropertyForm: string = '';
  curProductProperty: ProductProperty | undefined;


  uniquePropertyNames: string[] = []
  searchKeyword: string = '';
  filterPropertyName: string = '';

  totalProducts = 0;
  pageSize = 10;
  currentPage = 0;

  isConFirmDisplayed: boolean = false;
  isFormPropertyDisplayed: boolean = false;
  isOverlayDisplayed: boolean = false;
  displayedColumns: string[] = ['ID', 'TÊN', 'MÔ TẢ', 'HÀNH ĐỘNG'];

  dataSource = new MatTableDataSource<ProductProperty>();

  private searchInputSubject = new Subject<string>();

  private productNotificationHubUrl = 'http://localhost:5293/gateway/product-apis/productPropertyHub';
  private destroyComponent$ = new Subject<void>();

  constructor(private productPropertyService: PropertyService, private signalrService: SignalRService) {


  }

  ngOnInit(): void {
    this.loadProductProperties();
    this.loadUniquePropertyNames();
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
    this.signalrService.on<any[]>(this.productNotificationHubUrl, 'ProductPropertyChanged')
      .pipe(takeUntil(this.destroyComponent$))
      .subscribe(
        response => {
          if (Array.isArray(response) && response.length >= 2) {
            const message = response[1]; // Lấy phần tử thứ hai của mảng

            if (message === 'ProductPropertyAdded') {
              this.loadUniquePropertyNames();
              this.loadProductProperties(); // Tải lại trang hiện tại
            } else if (message === 'ProductPropertyUpdated') { // Xử lý nếu có event update
              this.loadProductProperties();
            } else if (message === 'ProductPropertyDeleted') { // Xử lý nếu có event delete
              this.loadUniquePropertyNames();
              this.loadProductProperties();
            }
          } else {
            console.warn('SignalR: Unexpected response format:', response);
          }
        },
        error => {
          console.log(error)
        });
  }

  private loadUniquePropertyNames(): void {
    this.productPropertyService.getAllUniquePropertyNames()
      .pipe(takeUntil(this.destroyComponent$))
      .subscribe({
        next: (names) => {
          this.uniquePropertyNames = names;
          // console.log('Unique property names loaded:', names);
        },
        error: (error) => {
          console.error('Error loading unique property names:', error);
        }
      });
  }

  private loadProductProperties(): void {
    this.productPropertyService.getPagedProductProperties(this.currentPage + 1, this.pageSize, this.searchKeyword, this.filterPropertyName)
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
            console.error('Error loading product properties:', error);
          }
        }
      );
  }

  onPageChange(event: PageEvent): void {

    this.currentPage = event.pageIndex;
    console.log(this.currentPage)
    this.pageSize = event.pageSize;
    this.loadProductProperties();
  }

  onSearchChange(): void {
    this.currentPage = 0; // Reset về trang đầu tiên
    this.loadProductProperties();
  }

  onFilterChange(): void {
    this.currentPage = 0; // Reset về trang đầu tiên khi lọc mới
    this.loadProductProperties();
  }

  onSearchInput(event: Event): void {
    const inputValue = (event.target as HTMLInputElement).value;
    this.searchKeyword = inputValue; // Cập nhật biến searchKeyword ngay lập tức
    this.searchInputSubject.next(inputValue);
  }

  DeleteProductProperty() {
    if (this.curProductProperty) {
      this.productPropertyService.deleteProductProperty(this.curProductProperty.ID).subscribe({
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

  OnclickDelete(productProperty: ProductProperty) {
    this.DisplayOverlay(true);
    this.DisplayConfirmForm(true);
    this.curProductProperty = productProperty;
  }

  OnclickUpdate(productProperty: ProductProperty) {
    this.curProductProperty = productProperty;
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
    this.DeleteProductProperty();
  }

  ngOnDestroy(): void {
    this.destroyComponent$.next();
    this.destroyComponent$.complete();
    this.signalrService.stopAllConnections();
  }
}
