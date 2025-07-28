import { Component, ElementRef, HostListener, ViewChild } from '@angular/core';
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

  isLoading: boolean = false;
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

  @HostListener('window:scroll', ['$event'])
  onWindowScroll(): void {
    // console.log("Window Scroll Event Fired!"); // Để debug

    // document.documentElement.scrollHeight: Tổng chiều cao nội dung của toàn bộ trang
    // window.scrollY (hoặc window.pageYOffset): Vị trí cuộn hiện tại từ đỉnh
    // window.innerHeight: Chiều cao hiển thị của cửa sổ trình duyệt (viewport)

    // Điều kiện để xác định cuộn đến cuối trang:
    // Khi tổng chiều cao nội dung trừ đi vị trí cuộn hiện tại
    // NHỎ HƠN HOẶC BẰNG chiều cao của viewport cộng với một ngưỡng (ví dụ 100px)
    if (window.innerHeight + window.scrollY >= document.body.scrollHeight - 100) {
      if (!this.isLoading && this.page <= this.totalPage) {
        // console.log("Reached end of page, loading more products..."); // Để debug
        this.loadProducts();
      }
    }
  }
  // --- Hàm tải sản phẩm ---
  private loadProducts(): void {
    // Tránh tải nếu đang trong quá trình tải hoặc đã tải hết trang
    if (this.isLoading || (this.totalPage > 0 && this.page > this.totalPage)) {
      return;
    }

    this.isLoading = true; // Đặt trạng thái đang tải

    this.productService.getPagedProducts(
      this.page,
      this.pageSize,
      null, // Tham số queryName của bạn
      this.productTypeName,
      this.productBrandName,
      this.productStatusSort,
      this.productPropertyIDS,
      this.GetPriceSortStatus() // Hàm này cần được định nghĩa trong component của bạn
    ).subscribe(
      {
        next: (response: any) => { // Cần xác định kiểu dữ liệu của response
          if (response && response.Items) {
            this.products.push(...response.Items); // Thêm sản phẩm mới vào danh sách hiện có
            this.page++; // Tăng số trang lên để tải trang tiếp theo
            this.totalPage = response.TotalPages; // Cập nhật tổng số trang

            // Đặt lại isLoading sau khi tải xong dữ liệu
            this.isLoading = false;
            this.canShowNotFound = true; // Có thể bật cờ này nếu có dữ liệu

            console.log('Đã tải sản phẩm:', response);
          } else {
            // Xử lý trường hợp response không hợp lệ (ví dụ: không có Items)
            console.warn('Response không có thuộc tính Items hoặc không hợp lệ', response);
            this.isLoading = false;
          }
        },
        error: (error: any) => {
          console.error('Lỗi khi tải sản phẩm:', error);
          this.isLoading = false; // Đặt lại isLoading khi có lỗi
          // Có thể hiển thị thông báo lỗi cho người dùng ở đây
        }
      }
    );
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

