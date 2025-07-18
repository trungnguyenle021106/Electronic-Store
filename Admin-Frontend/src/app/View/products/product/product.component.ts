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



  curProduct: ProductDTO | undefined;
  productDTOs: ProductDTO[] = [];
  displayedColumns: string[] = ['ID', 'Name', 'Image', 'ProductBrandName', 'ProductTypeName', 'Quantity', 'Price', 'Status', 'Actions'];
  dataSource = new MatTableDataSource<ProductDTO>();
  searchValue: string = '';
  filterValue: string = '';
  readonly dialog = inject(MatDialog);


  constructor(private router: Router, private productService: ProductService) { }

  ngOnInit(): void {
    this.loadUProducts();
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

  private loadUProducts(): void {
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

  DeleteProduct() {
    if (this.curProduct) {
      this.productService.deleteProduct(this.curProduct.ID).subscribe({
        next: (response) => {
          this.loadUProducts();
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
        productID: product.ID
      }
    });
  }

  OnClickDeleteProduct(product: ProductDTO): void {
    this.curProduct = product;
    this.openConfirmDialog("300ms", "150ms", "xóa sản phẩm");
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadUProducts();
  }

  onSearchInput(event: Event): void {
    const inputValue = (event.target as HTMLInputElement).value;
    this.searchValue = inputValue; // Cập nhật biến searchKeyword ngay lập tức
    this.searchInputSubject.next(inputValue);
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.loadUProducts();
  }

  onFilterChange(): void {
    this.currentPage = 0;
    this.loadUProducts();
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
