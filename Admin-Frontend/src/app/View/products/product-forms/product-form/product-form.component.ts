import { Component, inject, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { ProductProperty } from '../../../../Model/Product/ProductProperty';
import { ProductType } from '../../../../Model/Product/ProductType';
import { ProductTypeService } from '../../../../Service/Product/product-type.service';
import { ProductBrand } from '../../../../Model/Product/ProductBrand';
import { ProductBrandService } from '../../../../Service/Product/product-brand.service';
import { ProductService } from '../../../../Service/Product/product.service';
import { Product } from '../../../../Model/Product/Product';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../dialogs/confirm-dialog/confirm-dialog.component';
import { ErrorDialogComponent } from '../../../dialogs/error-dialog/error-dialog.component';
import { ProductDTO } from '../../../../Model/Product/DTO/Response/ProductDTO';
import { Subject, takeUntil } from 'rxjs';



@Component({
  selector: 'app-product-form',
  standalone: false,
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.css'
})
export class ProductFormComponent {
  readonly dialog = inject(MatDialog);

  productForm: FormGroup;
  selectedFile?: File | null = null;
  selectedImage: string | ArrayBuffer | null = null; // Lưu trữ URL của ảnh đã chọn
  imageFileName: string = "Chưa có hình ảnh nào được chọn"; // Tên file hiển thị
  fileInput: HTMLInputElement | null = null; // Tham chiếu đến input type="file"
  urlImageUpdate: string = "";

  curProductID: number | undefined
  typeProductForm: string = "";
  selectedProperties: ProductProperty[] = [];
  senderSelectedItem: ProductProperty | undefined;
  senderUnSelectedItem: ProductProperty | undefined;
  actionName: string = "";
  describeAction: string = "";


  productTypes: ProductType[] = [];
  productBrands: ProductBrand[] = [];
  productStatus: string[] = ['Còn hàng', 'Hết hàng', 'Ngừng kinh doanh']

  private destroyComponent$ = new Subject<void>();
  constructor(private ProductService: ProductService, private ProductTypeService: ProductTypeService, private ProductBrandService: ProductBrandService, private fb: FormBuilder,
    private route: ActivatedRoute, private router: Router
  ) {
    this.productForm = this.fb.group({
      Name: ['', Validators.required],
      Price: [null, [Validators.required, Validators.min(0)]],
      Quantity: [null, [Validators.required, Validators.min(1)]],
      Description: ['', Validators.required],
      Status: ['', Validators.required],
      ProductTypeID: ['', Validators.required],
      ProductBrandID: ['', Validators.required],
    });
  }

  ngOnInit() {
    this.getProductBrands();
    this.getProductTypes();
    this.route.queryParams.subscribe(params => {
      this.typeProductForm = params['typeProductForm'];

      if (this.typeProductForm == "Create") {
        this.setForCreate();
      }
      else if (this.typeProductForm == "Update") {
        const productID: number = params['itemID'];
        this.curProductID = productID;
        this.setForUpdate(productID);
      }
    });
  }

  ngAfterViewInit() {

  }


  onSubmit(): void {
    this.productForm.markAllAsTouched();
    if (this.selectedProperties.length == 0 && this.typeProductForm == "Create") {
      this.openErrorDialog("300ms", "150ms", "Lỗi thêm sản phẩm", "Sản phẩm phải chứa ít nhất 1 thuộc tính")
    }
    else if (this.selectedProperties.length == 0 && this.typeProductForm == "Update") {
      this.openErrorDialog("300ms", "150ms", "Lỗi cập nhật sản phẩm", "Sản phẩm phải chứa ít nhất 1 thuộc tính")
    }
    else if (this.productForm.valid && this.selectedFile && this.typeProductForm == "Create") {
      this.openConfirmDialog("300ms", "150ms", "thêm sản phẩm");
    } else if (this.productForm.valid && (this.selectedFile || this.urlImageUpdate != "") && this.typeProductForm == "Update") {
      this.openConfirmDialog("300ms", "150ms", "cập nhật sản phẩm");
    }
  }

  handleConfirmAction() {

    if (this.typeProductForm == "Create") {
      this.CreateProduct();
    } else if (this.typeProductForm == "Update") {
      this.UpdateProduct();
    }
  }

  CreateProduct(): void {
    const newProduct: Product = {
      ID: 0,
      Name: this.productForm.value.Name,
      Price: this.productForm.value.Price,
      Quantity: this.productForm.value.Quantity,
      Description: this.productForm.value.Description,
      Status: this.productForm.value.Status,
      ProductTypeID: this.productForm.value.ProductTypeID,
      ProductBrandID: this.productForm.value.ProductBrandID,
      Image: ''
    }
    const productPropertyIDs: number[] = this.selectedProperties.map(prop => prop.ID);
    if (this.selectedFile) {
      this.ProductService.createProduct(newProduct, productPropertyIDs, this.selectedFile).subscribe({
        next: (response) => {
          this.router.navigate(['product'])
        }, error: (error) => {
          this.openErrorDialog("300ms", "150ms", "Lỗi thêm sản phẩm", error.message)
        }
      });
    }
  }

  UpdateProduct(): void {
    const newProduct: Product = {
      ID: this.curProductID ?? 0,
      Name: this.productForm.value.Name,
      Price: this.productForm.value.Price,
      Quantity: this.productForm.value.Quantity,
      Description: this.productForm.value.Description,
      Status: this.productForm.value.Status,
      ProductTypeID: this.productForm.value.ProductTypeID,
      ProductBrandID: this.productForm.value.ProductBrandID,
      Image: this.urlImageUpdate
    }

    const productPropertyIDs: number[] = this.selectedProperties.map(prop => prop.ID);
    if (this.selectedFile || this.urlImageUpdate) {
      this.ProductService.updateProduct(newProduct.ID, newProduct, productPropertyIDs, this.selectedFile ?? null).subscribe({
        next: (response) => {
          this.router.navigate(['product'])
        }, error: (error) => {
          this.openErrorDialog("300ms", "150ms", "Lỗi cập nhật sản phẩm", error.message)
          console.log(error)
        }
      });
    }
  }


  getProductTypes() {
    this.ProductTypeService.getPagedProductTypes(1, 10).subscribe({
      next: (response) => {
        this.productTypes = response.Items;
      },
      error: (error) => {
        console.log(error);
      }
    });
  }

  getProductBrands() {
    this.ProductBrandService.getPagedProductBrands(1, 10).subscribe({
      next: (response) => {
        this.productBrands = response.Items;
      },
      error: (error) => {
        console.log(error);
      }
    });
  }
  // CHỌN HÌNH ẢNH 

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files[0]) {
      const file = input.files[0];
      this.selectedFile = file;

      const reader = new FileReader();

      reader.onload = (e) => {
        this.selectedImage = e.target?.result || null;
      };

      reader.readAsDataURL(file);
      this.imageFileName = file.name;
    } else {
      this.clearImage(); // Xóa ảnh nếu không có file nào được chọn
    }
  }

  clearImage(): void {
    this.selectedImage = null;
    this.imageFileName = "Chưa có hình ảnh nào được chọn";
    this.selectedFile = null;
    // Đặt lại giá trị của input type="file" để có thể chọn lại cùng một file
    if (this.fileInput) {
      this.fileInput.value = '';
    }
  }

  private setForCreate() {
    this.actionName = "Thêm sản phẩm"
    this.describeAction = "Thêm sản phẩm"
  }

  private setForUpdate(productID: number) {
    this.actionName = "Cập nhật"
    this.describeAction = "Cập nhật sản phẩm"
    this.ProductService.getProductByID(productID).subscribe({
      next: (response) => {
        this.productForm.patchValue(response);
        if (response.Image) {
           const timestamp = Date.now()
          this.selectedImage = response.Image  ;
          this.urlImageUpdate = response.Image;
          const lastSlashIndex = response.Image.lastIndexOf('/');
          const imageName = response.Image.substring(lastSlashIndex + 1);
          this.imageFileName = imageName;
        }
      }, error: (error) => {
        console.log(error)
      }
    });
    this.loadselectProductProperties();
  }


  private loadselectProductProperties(): void {
    if (this.curProductID) {
      this.ProductService.getAllPropertiesOfProduct(this.curProductID)
        .pipe(takeUntil(this.destroyComponent$)) // Hủy đăng ký khi component bị hủy
        .subscribe(
          {
            next: (response) => {
              this.selectedProperties = response;
            },
            error: (error) => {
              console.error('Error loading product properties:', error);
            }
          }
        );
    }
  }

  openConfirmDialog(enterAnimationDuration: string, exitAnimationDuration: string, actionName: string): void {
    this.dialog.open(ConfirmDialogComponent, {
      width: '300px', // Tăng width để dễ nhìn hơn
      enterAnimationDuration,
      exitAnimationDuration,
      // ✨ TRUYỀN PHƯƠNG THỨC VÀO ĐÂY QUA THUỘC TÍNH 'data' ✨
      data: {
        actionName: actionName,
        onConfirm: () => this.handleConfirmAction()
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

  OnItemSelected(productProperty: ProductProperty) {
    const item: ProductProperty = { ID: productProperty.ID, Name: productProperty.Name, Description: productProperty.Description };
    this.senderSelectedItem = item;
    this.selectedProperties.push(productProperty);
  }

  OnItemUnSelected(productProperty: ProductProperty) {
    const item: ProductProperty = { ID: productProperty.ID, Name: productProperty.Name, Description: productProperty.Description };
    this.senderUnSelectedItem = item;
     this.selectedProperties = this.selectedProperties.filter(item => item.ID !== productProperty.ID);
  }


  ngOnDestroy(): void {
    this.destroyComponent$.next();
    this.destroyComponent$.complete();
  }
}
