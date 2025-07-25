import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductProperty } from '../../Model/Product/ProductProperty';
import { ProductService } from '../../Service/Product/product.service';
import { Product } from '../../Model/Product/Product';
import { ContentManagementService } from '../../Service/ContentManagement/content-management.service';

@Component({
  selector: 'app-product',
  standalone: false,
  templateUrl: './product.component.html',
  styleUrl: './product.component.css'
})
export class ProductComponent {
  page: number = 1;
  pageSize: number = 10;
  totalPage: number = 1;

  filterID: number = 0;
  productTypeName: string = "";
  productBrandName: string = "";
  productPropertyIDS: string = "";
  productPropertyID: number = 0;
  productPriceSortOrder: string = "increase";
  productStatusSort: string = "";

  canShowNotFound: boolean = false;

  selectedFilters: { [key: string]: number | null } = {};
  products: Product[] = [];
  productPropertyOfFilter: ProductProperty[] = []
  uniqueProductPropertyName: Set<string> = new Set<string>();

  constructor(private route: ActivatedRoute, private productService: ProductService, private contentManagementService: ContentManagementService) {
  }

  ngOnInit() {
    this.route.queryParamMap.subscribe(params => {
      const newProductTypeName = params.get('ProductTypeName') ?? '';
      const newProductPropertyID = Number(params.get('ProductPropertyID'));
      const newFilterID = Number(params.get('FilterID'));

      // Kiểm tra xem các param có thực sự thay đổi không để quyết định tải lại properties
      const filterParamsChanged =
        this.filterID !== newFilterID ||
        this.productPropertyID !== newProductPropertyID ||
        this.productTypeName !== newProductTypeName; // Thêm cả productTypeName vào điều kiện này nếu nó ảnh hưởng đến filter properties

      // Cập nhật các biến trạng thái
      this.productTypeName = newProductTypeName;
      this.productPropertyID = newProductPropertyID;
      this.filterID = newFilterID;

      // LUÔN reset pagination và sản phẩm khi query params thay đổi
      this.resetPaginationAndProducts();

      // Chỉ tải lại product properties và reset các biến liên quan nếu các tham số lọc thay đổi
      if (filterParamsChanged) {
        this.resetFilterPropertiesState(); // <<< ĐÂY LÀ ĐIỂM THAY ĐỔI QUAN TRỌNG
        this.loadProductPropertyOfFilterr();
      } else {
        // Nếu các tham số lọc không đổi, nhưng có thể các tham số khác (như trang, sắp xếp) thay đổi
        // thì vẫn cần tải lại sản phẩm (dựa trên trạng thái hiện tại của filter properties)
        this.loadProducts();
      }
    });
  }
  
 private resetFilterPropertiesState() {
    this.productPropertyOfFilter = []; // Xóa mảng properties hiện có
    this.uniqueProductPropertyName.clear(); // Xóa tất cả các tên thuộc tính duy nhất
    this.selectedFilters = {}; // Reset tất cả các lựa chọn bộ lọc
    this.productBrandName = ""; // Reset brand name
    this.productPropertyIDS = ""; // Reset chuỗi IDs
  }

  private resetPaginationAndProducts() {
    this.page = 1;
    this.totalPage = 1;
    this.products = [];
    this.canShowNotFound = false; // Reset trạng thái tìm thấy
  }

  private loadProducts() {
    if (this.totalPage >= this.page) {
      this.productService.getPagedProducts(this.page, this.pageSize, null, this.productTypeName,
        this.productBrandName, this.productStatusSort, this.productPropertyIDS, this.GetPriceSortStatus()).subscribe(
          {
            next: (response) => {
              this.products.push(...response.Items);
              this.page++;
              this.totalPage = response.TotalPages;
              this.canShowNotFound = true;
              console.log(response)
            },
            error: (error) => {
              console.log(error)
            }
          }
        );
    }
  }

  private loadProductPropertyOfFilterr() {
    this.contentManagementService.getAllPropertiesOfFilter(this.filterID).subscribe({
      next: (response) => {

        this.productPropertyOfFilter = response;
        const productProperty = this.productPropertyOfFilter.find(pp => pp.ID == this.productPropertyID)
        if (productProperty && productProperty.Name.toLowerCase() === "thương hiệu") {
          this.productBrandName = productProperty.Description;
          this.productPropertyIDS = "";
          let proProps: ProductProperty[] = [];
          response.forEach(pp => {
            if (!(pp.Name.toLowerCase() === "thương hiệu")) {
              proProps.push(pp)
            }
          });
          this.productPropertyOfFilter = proProps;

        } else if (productProperty) {
          this.productPropertyIDS = productProperty?.ID ? String(productProperty.ID) : "";
          this.productBrandName = "";
        }

        this.productPropertyOfFilter.forEach(pp => {
          this.uniqueProductPropertyName.add(pp.Name);
          if (this.selectedFilters[pp.Name] === undefined) { // Chỉ khởi tạo nếu chưa có giá trị
            this.selectedFilters[pp.Name] = null;
          }
        });


        this.loadProducts();
      },
      error: (error) => {
        console.log(error)
      }
    })
  }


  GetProductPropertiesByName(propName: string): ProductProperty[] {
    let array: ProductProperty[] = [];
    this.productPropertyOfFilter.forEach(pp => {
      if (pp.Name == propName) {
        array.push(pp);
      }
    });
    return array;
  }

  private GetPriceSortStatus(): boolean {
    if (this.productPriceSortOrder === "increase") {
      return true;
    }
    return false;
  }

  onFilterProductPropertyChange(propName: string, selectedId: number | null): void {
    this.selectedFilters[propName] = selectedId;

    let propertyIDsToJoin: number[] = []; // Mảng để chứa các ID số của các thuộc tính (trừ "thương hiệu")
    let newProductBrandName: string | undefined = undefined; // Biến tạm cho brand name

    // Duyệt qua các key-value trong selectedFilters để tổng hợp tất cả lựa chọn
    for (const key in this.selectedFilters) {
      // Đảm bảo chỉ xử lý các thuộc tính trực tiếp của object, không phải thuộc tính kế thừa
      if (Object.prototype.hasOwnProperty.call(this.selectedFilters, key)) {
        const filterValue = this.selectedFilters[key];

        // Nếu bộ lọc có giá trị được chọn (không phải null)
        if (filterValue !== null) {
          const fullProp = this.productPropertyOfFilter.find(p => p.ID == filterValue);
          if (fullProp) {
            propertyIDsToJoin.push(filterValue);
          }
        }
      }
    }

    this.canShowNotFound = false;
    // Gán giá trị cuối cùng cho các biến của component

    this.productPropertyIDS = propertyIDsToJoin.length > 0 ? propertyIDsToJoin.join(',') : ''; // Gán chuỗi rỗng nếu không có ID nào
    this.productBrandName = newProductBrandName ?? ''; // Gán chuỗi rỗng nếu không có brand name
    this.page = 1; // Luôn reset page khi các bộ lọc thay đổi
    this.totalPage = 1;
    this.products = []; // Xóa sản phẩm cũ để tải lại từ đầu
    this.loadProducts();
  }

  OnFilterChange() {
    this.canShowNotFound = false;
    this.page = 1; // Luôn reset page khi các bộ lọc thay đổi
    this.totalPage = 1;
    this.products = []; // Xóa sản phẩm cũ để tải lại từ đầu
    this.loadProducts();
  }
}

