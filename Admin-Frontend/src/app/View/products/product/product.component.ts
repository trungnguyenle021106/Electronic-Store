import { Component, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ProductDTO } from '../../../Model/Product/DTO/Response/ProductDTO';
import { ProductService } from '../../../Service/Product/product.service';




@Component({
  selector: 'app-product',
  standalone: false,
  templateUrl: './product.component.html',
  styleUrl: './product.component.css'
})
export class ProductComponent {
  @ViewChild('scrollContainer') scrollContainerRef!: ElementRef<HTMLDivElement>;
  loading: boolean = false;

  startID: number = 0; // bằng với đợt lấy id phần tử đầu tiên trong mảng ProductDTO
  lastID: number = 0;// bằng với đợt lấy id phần tử cuối trong mảng ProductDTO
  HasPreviousPage: boolean = false;
  HasNextPage: boolean = true;

  page: number = 1;
  pageSize: number = 10;

  isCreateProductDisplay: boolean = false;

  productDTOs: ProductDTO[] = [];

  private intersectionObserver!: IntersectionObserver;
  visibleProductIDs: Set<number> = new Set<number>();

  constructor(private router: Router, private productService: ProductService) { }

  ngOnInit(): void {
    this.productDTOs = Array.from({ length: 10 }, (_, i) => ({
      ID: i + 1,
      Name: `Laptop gaming MSI Katana 15 B13VFK ${i + 600}VN`,
      Image: `https://product.hstatic.net/200000722513/product/676vn_21da8c4630014f808b321b3d32118291_69f68ad8d3be44b385bb3da80ec4a9ee_1024x1024.png`,
      ProductBrandName: (i % 2 === 0) ? 'ASUS' : 'MSI',
      ProductTypeName: 'LAPTOP',
      Quantity: 10000 + i * 100,
      Price: 25000000 + i * 500000,
      Status: (i % 3 === 0) ? 'Còn hàng' : (i % 3 === 1 ? 'Hết hàng' : 'Đang về')
    }));

    this.setStartLastID();
  }

  ngAfterViewInit(): void {
    if (this.scrollContainerRef && this.scrollContainerRef.nativeElement) {
      const scrollContainer = this.scrollContainerRef.nativeElement;

      // --- Khởi tạo Intersection Observer ---
      const options: IntersectionObserverInit = {
        root: scrollContainer, // Phần tử cuộn là root
        rootMargin: '0px',
        threshold: 1// Kích hoạt callback khi 10% của phần tử hiển thị
      };

      this.intersectionObserver = new IntersectionObserver((entries: IntersectionObserverEntry[]) => {
        entries.forEach(entry => {
          const productId = (entry.target as HTMLElement).dataset['productId']; // Lấy ID từ data-attribute
          if (productId) {
            const id = parseInt(productId, 10);
            if (entry.isIntersecting) {
              this.visibleProductIDs.add(id);
              // console.log(`Hàng ID: ${id} đang hiển thị.`);
            } else {
              this.visibleProductIDs.delete(id);
              // console.log(`Hàng ID: ${id} đã thoát khỏi tầm nhìn.`);
            }
          }
        });
        this.handleElementOutBound();
        // Console log tất cả các ID sản phẩm đang hiển thị sau mỗi lần cập nhật
        console.log('Sản phẩm đang hiển thị:', Array.from(this.visibleProductIDs).sort((a, b) => a - b));
      }, options);

      // --- Quan sát từng hàng ---
      // Cần chờ Angular render xong các hàng
      setTimeout(() => {
        const rows = scrollContainer.querySelectorAll('tbody tr');
        rows.forEach(row => {
          this.intersectionObserver.observe(row);
        });
      }, 50); // setTimeout(..., 0) để đảm bảo các phần tử DOM đã được thêm vào bảng

      // scrollContainer.addEventListener('scroll', this.onScroll);
    }
  }

  // Phương thức xử lý sự kiện cuộn (để tải thêm dữ liệu)
  // private onScroll = () => { // Sử dụng arrow function để giữ ngữ cảnh 'this'
  //   if (this.scrollContainerRef && this.scrollContainerRef.nativeElement) {
  //     const scrollContainer = this.scrollContainerRef.nativeElement;
  //     const scrollThreshold = 1;
  //     if (scrollContainer.scrollTop + scrollContainer.clientHeight >= scrollContainer.scrollHeight - scrollThreshold) {
  //       if (!this.loading) {
  //         console.log('Đã cuộn tới cuối của container chứa bảng! Đang tải thêm...');
  //         // this.loadMoreData();
  //       }
  //     }
  //   }
  // };
  private handleElementOutBound(): void {
    let isStart: boolean = true;
    if (this.visibleProductIDs.has(this.startID)) {
      isStart = true;
      console.log("đầu mảng");
    } else if (this.visibleProductIDs.has(this.lastID)) {
      isStart = false;
      console.log("cuối mảng");
    }

    const temp = this.productDTOs.length + this.pageSize;
    const remainder = temp - this.productDTOs.length;
    if (remainder != 0 && isStart && this.HasPreviousPage) {
      const startNumber = this.productDTOs.length - this.pageSize - 1;
      const endNumber = this.productDTOs.length - 1
      this.productDTOs.splice(startNumber, endNumber);
      this.loadMoreData(false);
    }
    else if (remainder != 0 && !isStart && this.HasNextPage) {
      const startNumber = 0;
      const endNumber = this.pageSize - 1;
      this.productDTOs.splice(startNumber, endNumber);
      this.loadMoreData(true);
      this.HasNextPage = false
    }
  }

  loadMoreData(isLoadNext: boolean): void {
    this.loading = true;
    setTimeout(() => {
      const startIndex = this.productDTOs.length;
      const newItemsCount = this.pageSize; // Thêm 15 mục mỗi lần
      const newProducts: ProductDTO[] = [];
      for (let i = 0; i < newItemsCount; i++) {
        const newId = startIndex + i + 1;
        let newProduct = {
          ID: newId,
          Name: `Laptop gaming MSI Katana 15 B13VFK ${newId + 600}VN`,
          Image: `https://product.hstatic.net/200000722513/product/676vn_21da8c4630014f808b321b3d32118291_69f68ad8d3be44b385bb3da80ec4a9ee_1024x1024.png`,
          ProductBrandName: (newId % 2 === 0) ? 'ASUS' : 'MSI',
          ProductTypeName: 'LAPTOP',
          Quantity: 10000 + newId * 100,
          Price: 25000000 + newId * 500000,
          Status: (newId % 3 === 0) ? 'Còn hàng' : (newId % 3 === 1 ? 'Hết hàng' : 'Đang về')
        };
        if (isLoadNext) {
          newProduct.Name += "next";
        } else {
          newProduct.Name += "prev";
        }
        newProducts.push(newProduct);
        this.productDTOs.push(newProduct);
      }
      this.setStartLastID();
      this.loading = false;

      console.log('Đã thêm dữ liệu mới. Tổng số sản phẩm:', this.productDTOs.length);

      // Quan trọng: Quan sát các hàng mới được thêm vào DOM
      // Cần đợi Angular cập nhật DOM sau khi this.productDTOs thay đổi
      setTimeout(() => {
        if (this.scrollContainerRef && this.scrollContainerRef.nativeElement && this.intersectionObserver) {
          const newRows = this.scrollContainerRef.nativeElement.querySelectorAll(`tbody tr[data-product-id$="${startIndex + newItemsCount}"] ~ tr`);
          // Cách trên lấy các hàng sau hàng cuối cùng của đợt trước.
          // Một cách đơn giản hơn nếu bạn chỉ quan tâm đến các hàng mới:
          // newProducts.forEach(p => {
          //   const rowElement = this.scrollContainerRef.nativeElement.querySelector(`tbody tr[data-product-id="${p.ID}"]`);
          //   if (rowElement) {
          //     this.intersectionObserver.observe(rowElement);
          //   }
          // });
          newRows.forEach(row => {
            this.intersectionObserver.observe(row);
          });
        }
      }, 0);
    }, 500);
  }


  private setStartLastID(): void {
    this.startID = this.productDTOs[0].ID;
    this.lastID = this.productDTOs[this.productDTOs.length - 1].ID;
  }

  OnClickCreateProduct(): void {
    // this.router.navigate(['create-product']);
        this.router.navigate(['product-form']);
  }

  ngOnDestroy(): void {
    // Rất quan trọng: Hủy quan sát Intersection Observer và gỡ bỏ listener khi component bị hủy
    if (this.intersectionObserver) {
      this.intersectionObserver.disconnect(); // Ngừng quan sát tất cả các phần tử
    }
    // if (this.scrollContainerRef && this.scrollContainerRef.nativeElement && this.onScroll) {
    //   this.scrollContainerRef.nativeElement.removeEventListener('scroll', this.onScroll);
    // }
  }
}
