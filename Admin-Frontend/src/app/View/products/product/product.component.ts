import { Component, ElementRef, inject, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ProductDTO } from '../../../Model/Product/DTO/Response/ProductDTO';
import { ProductService } from '../../../Service/Product/product.service';
import { MatTableDataSource } from '@angular/material/table';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { ConfirmDialogComponent } from '../../dialogs/confirm-dialog/confirm-dialog.component';
import { ErrorDialogComponent } from '../../dialogs/error-dialog/error-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { Product } from '../../../Model/Product/Product';
import { ProductTypeService } from '../../../Service/Product/product-type.service';
import { ProductBrandService } from '../../../Service/Product/product-brand.service';
import { ProductType } from '../../../Model/Product/ProductType';
import { ProductBrand } from '../../../Model/Product/ProductBrand';




@Component({
  selector: 'app-product',
  standalone: false,
  templateUrl: './product.component.html',
  styleUrl: './product.component.css'
})
export class ProductComponent {
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  private destroyComponent$ = new Subject<void>();
  private searchInputSubject = new Subject<string>();
  pageSize = 5;
  currentPage = 0;

  productTypes: ProductType[] = [];
  productBrands: ProductBrand[] = [];
  curProduct: Product | undefined;
  // productDTOs: ProductDTO[] = [];
  displayedColumns: string[] = ['ID', 'Name', 'Image', 'ProductBrandName', 'ProductTypeName', 'Quantity', 'Price', 'Status', 'Actions'];
  dataSource = new MatTableDataSource<Product>();
  searchValue: string = '';
  filterValue: string = '';
  readonly dialog = inject(MatDialog);


  constructor(private router: Router, private productService: ProductService, private productTypeService: ProductTypeService,
    private productBrandService: ProductBrandService) { }

  ngOnInit(): void {
    this.loadAllInitialData();
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


  private loadProducts(): void {
    this.productService.getPagedProducts(this.currentPage + 1, this.pageSize, this.searchValue, this.filterValue)
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

  private async loadAllInitialData(): Promise<void> {
    console.log('Bắt đầu tải tất cả dữ liệu ban đầu...');

    try {
      // Promise.all đợi tất cả các Promise trong mảng hoàn thành
      // Nếu bất kỳ Promise nào bị reject, Promise.all sẽ reject
      await Promise.all([
        this.loadProductBrand(), // Phương thức này giờ trả về Promise
        this.loadProductType()   // Phương thức này giờ trả về Promise
      ]);

      console.log('Product Brands và Product Types đã tải xong. Bắt đầu tải Products...');
      // Sau khi cả hai Promise trên hoàn thành, mới gọi loadProducts
      this.loadProducts();

    } catch (error) {
      console.error('Lỗi khi tải dữ liệu ban đầu (Brands hoặc Types):', error);
      // Lỗi đã được xử lý trong từng Promise con, nhưng catch này để chắc chắn

    } finally {

      console.log('Tải dữ liệu ban đầu hoàn tất (Promise.all đã hoàn thành).');
    }
  }


  private loadProductType(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.productTypeService.getPagedProductTypes().subscribe({
        next: (response) => {
          this.productTypes = response.Items;
          resolve(); // Báo hiệu Promise hoàn thành thành công
        },
        error: (error) => {
          console.error('[Component] Lỗi khi tải Product Types:', error);

          reject(error); // Báo hiệu Promise hoàn thành thất bại
        }
      });
    });
  }

  private loadProductBrand(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.productBrandService.getPagedProductBrands().subscribe({
        next: (response) => {
          this.productBrands = response.Items;
          resolve(); // Báo hiệu Promise hoàn thành thành công
        },
        error: (error) => {
          console.error('[Component] Lỗi khi tải Product Brands:', error);

          reject(error); // Báo hiệu Promise hoàn thành thất bại
        }
      });
    });
  }

  GetProductBrandName(productBrandID: number): string {
    // 1. Kiểm tra xem mảng this.productBrands đã được tải và có giá trị chưa
    // Nếu chưa, hoặc nếu nó rỗng, thì không thể tìm kiếm, trả về chuỗi rỗng ngay lập tức
    if (!this.productBrands || this.productBrands.length === 0) {
      return "";
    }

    // 2. Kiểm tra tham số productBrandID có hợp lệ không (ví dụ: lớn hơn 0)
    // Mặc dù lỗi hiện tại không phải do tham số, nhưng đây là một kiểm tra tốt
    if (productBrandID === null || productBrandID === undefined || productBrandID <= 0) {
      return "";
    }

    // 3. Thực hiện tìm kiếm trên mảng đã đảm bảo có dữ liệu
    const foundBrand = this.productBrands.find(
      brand => brand.ID === productBrandID
    );

    // 4. Trả về tên nếu tìm thấy, ngược lại trả về chuỗi rỗng
    if (foundBrand) {
      return foundBrand.Name;
    } else {
      return "";
    }
  }

  // Phương thức lấy tên loại sản phẩm từ ID
  GetProductTypeName(productTypeID: number): string {
    // 1. Kiểm tra xem mảng this.productTypes đã được tải và có giá trị chưa
    if (!this.productTypes || this.productTypes.length === 0) {
      return "";
    }

    // 2. Kiểm tra tham số productTypeID có hợp lệ không
    if (productTypeID === null || productTypeID === undefined || productTypeID <= 0) {
      return "";
    }

    // 3. Thực hiện tìm kiếm trên mảng đã đảm bảo có dữ liệu
    const foundType = this.productTypes.find(type => type.ID === productTypeID);

    // 4. Trả về tên nếu tìm thấy, ngược lại trả về chuỗi rỗng
    if (foundType) {
      return foundType.Name;
    } else {
      return "";
    }
  }

  DeleteProduct() {
    if (this.curProduct) {
      this.productService.deleteProduct(this.curProduct.ID).subscribe({
        next: (response) => {
          this.loadProducts();
        },
        error: (error) => {
          this.openErrorDialog("300ms", "150ms", "Lỗi xóa sản phẩm", error.message)
          console.log(error);
        }
      })
    }
  }


  OnClickCreateProduct(): void {
    this.router.navigate(['product-form'], {
      queryParams: {
        typeProductForm: "Create",
      }
    });
  }

  OnClickUpdateProduct(product: ProductDTO): void {
    this.router.navigate(['product-form'], {
      queryParams: {
        typeProductForm: "Update",
        itemID: product.ID
      }
    });
  }

  OnClickDeleteProduct(product: Product): void {
    this.curProduct = product;
    this.openConfirmDialog("300ms", "150ms", "xóa sản phẩm");
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadProducts();
  }

  onSearchInput(event: Event): void {
    const inputValue = (event.target as HTMLInputElement).value;
    this.searchValue = inputValue; // Cập nhật biến searchKeyword ngay lập tức
    this.searchInputSubject.next(inputValue);
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.loadProducts();
  }

  onFilterChange(): void {
    this.currentPage = 0;
    this.loadProducts();
  }

  openConfirmDialog(enterAnimationDuration: string, exitAnimationDuration: string, actionName: string): void {
    this.dialog.open(ConfirmDialogComponent, {
      width: '300px', // Tăng width để dễ nhìn hơn
      enterAnimationDuration,
      exitAnimationDuration,
      // ✨ TRUYỀN PHƯƠNG THỨC VÀO ĐÂY QUA THUỘC TÍNH 'data' ✨
      data: {
        actionName: actionName,
        onConfirm: () => this.DeleteProduct()
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
