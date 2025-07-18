import { Component, EventEmitter, Input, Output, SimpleChanges, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { PropertyService } from '../../../../Service/Product/property.service';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { ProductProperty } from '../../../../Model/Product/ProductProperty';
import { ProductService } from '../../../../Service/Product/product.service';
import { Product } from '../../../../Model/Product/Product';




@Component({
  selector: 'app-select-product-properties',
  standalone: false,
  templateUrl: './select-product-properties.component.html',
  styleUrl: './select-product-properties.component.css'
})
export class SelectProductPropertiesComponent {
  @Input() selectedItemReceiver: ProductProperty | undefined;
  @Output() selectedItemSender = new EventEmitter<ProductProperty>();

  @Input() unSelectedItemReceiver: ProductProperty | undefined;
  @Output() unSelectedItemSender = new EventEmitter<ProductProperty>();

  @Input() curProductID: number | undefined;
  @Input() isSelectedPropertyForm: boolean = false;
  // --- Dữ liệu và logic cho thanh tìm kiếm và lọc HTML thuần ---

  uniquePropertyNames: string[] = []
  searchValue: string = '';
  filterValue: string = '';

  // --- Dữ liệu và logic cho MatTable và Paginator ---
  // Đổi tên cột từ 'select' thành 'actions' (hoặc 'chon')
  displayedColumns: string[] = ['id', 'name', 'Describe', 'actions'];

  dataSource = new MatTableDataSource<ProductProperty>();

  // selectedItems sẽ lưu trữ các mục đã chọn thủ công
  selectedItems: ProductProperty[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  pageSize = 5;
  currentPage = 0;

  private destroyComponent$ = new Subject<void>();
  private searchInputSubject = new Subject<string>();

  constructor(private productPropertyService: PropertyService, private productService: ProductService) {

  }

  ngOnInit() {
    if (!this.isSelectedPropertyForm) {
      this.loadUnselectProductProperties();
      this.loadUniquePropertyNames();
    }
    else if (this.isSelectedPropertyForm && this.curProductID) {
      this.loadselectProductProperties();
    }

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

  ngAfterViewInit() {
    if (this.isSelectedPropertyForm) {
      this.dataSource.paginator = this.paginator;
    }

    this.paginator.page
      .pipe(takeUntil(this.destroyComponent$))
      .subscribe((event: PageEvent) => {
        this.onPageChange(event);
      });
  }

  ngOnChanges(changes: SimpleChanges): void { // <-- ngOnChanges ở đây
    if (this.isSelectedPropertyForm) {

      this.HandleSelectedItemReceiver(changes);

    } else if (!this.isSelectedPropertyForm) {
      this.HandleuUnSelectedItemReceiver(changes);
    }

    if (changes['curProductID'] && changes['curProductID'].currentValue) {
      this.HandleCurProductID(changes);
    }

  }

  private HandleCurProductID(changes: SimpleChanges) {
    this.curProductID = changes['curProductID'].currentValue;
    this.loadselectProductProperties();
  }

  private HandleSelectedItemReceiver(changes: SimpleChanges) {
    if (changes['selectedItemReceiver'] && changes['selectedItemReceiver'].currentValue) {
      const newItem = changes['selectedItemReceiver'].currentValue;
      this.dataSource.data = [...this.dataSource.data, newItem]; // <<< THAY ĐỔI QUAN TRỌNG NHẤT

      if (this.paginator) {
        this.paginator.length = this.dataSource.data.length;
        const totalPages = Math.ceil(this.dataSource.data.length / this.paginator.pageSize);
        this.paginator.pageIndex = totalPages - 1; // Chuyển đến trang cuối cùng
        this.paginator._changePageSize(this.paginator.pageSize);
      }
    } else if (changes['selectedItemReceiver']) {
      console.log('Receiver: Dữ liệu đã bị xóa hoặc không có.');
    }
  }

  private HandleuUnSelectedItemReceiver(changes: SimpleChanges) {

    if (changes['unSelectedItemReceiver'] && changes['unSelectedItemReceiver'].currentValue) {

      const newItem = changes['unSelectedItemReceiver'].currentValue;
      const index = this.selectedItems.findIndex(item => item.ID === newItem.ID);
      if (index > -1) {
        // Nếu đã chọn, bỏ chọn
        this.selectedItems.splice(index, 1);
      }
    } else if (changes['unSelectedItemReceiver']) {
      console.log('Receiver: Dữ liệu đã bị xóa hoặc không có.');
    }
  }

  // --- Logic cho việc chọn hàng bằng nút bấm ---
  toggleSelection(row: ProductProperty) {
    if (!this.isSelectedPropertyForm) {
      // Nếu chưa chọn, thêm vào
      this.selectedItems.push(row);
      this.selectedItemSender.emit(row);

    } else {

      this.dataSource.data = this.dataSource.data.filter(item => item !== row);

      if (this.paginator) {
        this.paginator.length = this.dataSource.data.length;
        const totalPages = Math.ceil(this.dataSource.data.length / this.paginator.pageSize);
        this.paginator.pageIndex = totalPages - 1; // Chuyển đến trang cuối cùng
        this.paginator._changePageSize(this.paginator.pageSize);
      }

      this.unSelectedItemSender.emit(row);
    }
  }

  // Kiểm tra xem hàng có đang được chọn hay không (để đổi màu nút)
  isSelected(row: ProductProperty): boolean {
    return this.selectedItems.some(item => item.ID === row.ID);
  }

  private loadUnselectProductProperties(): void {
    this.productPropertyService.getPagedProductProperties(this.currentPage + 1, this.pageSize, this.searchValue, this.filterValue)
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

  private loadselectProductProperties(): void {
    if (this.curProductID) {
      this.productService.getAllPropertiesOfProduct(this.curProductID)
        .pipe(takeUntil(this.destroyComponent$)) // Hủy đăng ký khi component bị hủy
        .subscribe(
          {
            next: (response) => {
              if (this.isSelectedPropertyForm) {
                this.HandleLoadUpdateSelectForm(response);
              } else {
                this.HandleLoadUpdateUnselectForm(response);;
              }
            },
            error: (error) => {
              console.error('Error loading product properties:', error);
            }
          }
        );
    }
  }

  private HandleLoadUpdateUnselectForm(productProperties: ProductProperty[]) {
    this.selectedItems = productProperties;
  }

  private HandleLoadUpdateSelectForm(productProperties: ProductProperty[]) {
    this.dataSource.data = productProperties; // Cập nhật dữ liệu cho MatTable
    if (this.paginator) {
      this.paginator.length = productProperties.length; // Đảm bảo MatPaginator nhận giá trị totalProducts
      this.paginator.pageIndex = this.currentPage; // Đảm bảo MatPaginator hiển thị đúng trang\
    }
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

  onPageChange(event: PageEvent): void {

    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    if (!this.isSelectedPropertyForm) {
      this.loadUnselectProductProperties();
    }
  }

  onSearchInput(event: Event): void {
    const inputValue = (event.target as HTMLInputElement).value;
    this.searchValue = inputValue; // Cập nhật biến searchKeyword ngay lập tức
    this.searchInputSubject.next(inputValue);
  }

  onSearchChange(): void {
    this.currentPage = 0; // Reset về trang đầu tiên
    if (!this.isSelectedPropertyForm) {
      this.loadUnselectProductProperties();
    } else if (this.dataSource.data.length != 0) {
      this.dataSource.filterPredicate = (data: ProductProperty, filter: string) => {
        const searchMatch = data.Name.toLowerCase().includes(this.searchValue.toLowerCase()) ||
          data.Description.toLowerCase().includes(this.searchValue.toLowerCase());

        const filterMatch = this.filterValue === '' ||
          data.Description === this.filterValue;

        return searchMatch && filterMatch;
      };

      this.dataSource.filter = `${this.searchValue}-${this.filterValue}`;
    }
  }

  onFilterChange(): void {
    this.currentPage = 0; // Reset về trang đầu tiên khi lọc mới
    if (!this.isSelectedPropertyForm) {
      this.loadUnselectProductProperties();
    }
  }

  ngOnDestroy(): void {
    this.destroyComponent$.next();
    this.destroyComponent$.complete();
  }
}
