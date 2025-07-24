import { Component } from '@angular/core';
import { ProductService } from '../../Service/Product/product.service';
import { ActivatedRoute } from '@angular/router';
import { Product } from '../../Model/Product/Product';
import { ProductTypeService } from '../../Service/Product/product-type.service';
import { ProductType } from '../../Model/Product/ProductType';
import { ProductBrandService } from '../../Service/Product/product-brand.service';
import { ProductBrand } from '../../Model/ProductBrand';

@Component({
  selector: 'app-search',
  standalone: false,
  templateUrl: './search.component.html',
  styleUrl: './search.component.css'
})
export class SearchComponent {
  searchValue: string | null = null;
  page: number = 1;
  pageSize: number = 10;
  totalPage: number = 1;

  productPriceSortOrder: string = "increase";
  productStatusSort: string = "";
  productBrandSort: string = "";
  productTypeSort: string = "";
  productPropertyIDS: string = "";

  canShowNotFound: boolean = false;

  curProductTypes: ProductType[] = [];
  curProductBrands: ProductBrand[] = [];

  productTypes: ProductType[] = [];
  productBrands: ProductBrand[] = [];
  products: Product[] = []
  constructor(private route: ActivatedRoute, private productService: ProductService, private productTypeService: ProductTypeService,
    private productBrandService: ProductBrandService) {

  }

  ngOnInit() {

    this.route.queryParamMap.subscribe(params => {
      const newSearchValue = params.get('searchValue');


      if (this.searchValue !== newSearchValue) {
        this.searchValue = newSearchValue;
        this.resetSearchAndLoadProducts(); // Gọi hàm reset và tải lại
      } else if (this.products.length === 0 && this.searchValue) {

        // this.loadProducts(true); // Tải lần đầu, không phải do cuộn
        this.loadAllInitialData();
      }
    })
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
      this.loadProducts(true);

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


  private resetSearchAndLoadProducts(): void {
    this.products = []; // Xóa hết sản phẩm cũ
    this.page = 1;     // Reset về trang đầu tiên
    this.totalPage = 1; // Reset tổng số trang về 1 (sẽ được cập nhật khi fetch data)

    if (this.searchValue) {
      this.loadAllInitialData();; // Tải sản phẩm với từ khóa mới
    } else {
      // Tùy chọn: Xử lý khi không có searchValue (ví dụ: hiển thị thông báo, không hiển thị gì)
      console.log("No search value provided. Clearing results.");
    }
  }

  private loadProducts(isInitialLoad: boolean = false): void { // Thêm tham số tùy chọn
    // Nếu đây không phải lần tải ban đầu và đã hết trang, thì không làm gì
    if (!isInitialLoad && this.page > this.totalPage) {
      console.log("No more pages to load.");
      return;
    }

    // Luôn đảm bảo có searchValue để gọi API
    if (!this.searchValue) {
      console.warn("Cannot load products without a search value.");
      return;
    }
    this.productService.getPagedProducts(this.page, this.pageSize, this.searchValue, this.productTypeSort,
      this.productBrandSort, this.productStatusSort, this.productPropertyIDS, this.GetPriceSortStatus()).subscribe(
        {
          next: (response) => {
            // Sử dụng toán tử spread để tạo mảng mới, hoặc push nếu bạn muốn chỉnh sửa trực tiếp
            // this.products = [...this.products, ...response.Items]; // Tạo mảng mới
            this.products.push(...response.Items); // Thêm vào mảng hiện có
            this.page++; // Tăng số trang cho lần tải tiếp theo
            this.totalPage = response.TotalPages;
            this.canShowNotFound = true;
            // this.HandleCurProductType(response.Items);
            // this.HandleCurProductBrand(response.Items);
          },
          error: (error) => {
            console.error("Error loading products:", error);
            // Xử lý lỗi (ví dụ: hiển thị thông báo lỗi cho người dùng)
          }
        }
      );
  }

  private GetPriceSortStatus(): boolean {
    if (this.productStatusSort === "increase") {
      return true;
    }
    return false;
  }

  // private HandleCurProductType(products: Product[]) {
  //   products.forEach(element => {
  //     const productType: ProductType | undefined = this.productTypes.find(pt => pt.ID === element.ProductTypeID);
  //     if (productType && !this.curProductTypes.some(curPt => curPt.ID === productType.ID)) {
  //       this.curProductTypes.push(productType);
  //     }
  //   });
  // }

  // private HandleCurProductBrand(products: Product[]) {
  //   this.products.forEach(element => {
  //     const productBrand: ProductBrand | undefined = this.productBrands.find(pt => pt.ID === element.ProductBrandID);
  //     if (productBrand && !this.curProductBrands.some(curPt => curPt.ID === productBrand.ID)) {
  //       this.curProductBrands.push(productBrand);
  //     }
  //   });
  // }

  onSearch() {
    this.canShowNotFound = false;
    this.products = [];
    this.page = 1;
    this.totalPage = 1;
    this.loadProducts();
  }
}
