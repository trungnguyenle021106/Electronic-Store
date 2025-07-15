import { SelectionModel } from '@angular/cdk/collections';
import { Component, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
export interface ItemData {
  id: number;
  Name: string;
  Describe: string;
}

// Dữ liệu mẫu (có thể thay thế bằng dữ liệu từ API của bạn)
const ALL_ITEMS_DATA: ItemData[] = [
  { id: 1, Name: 'Màu', Describe: 'Vàng' },
  { id: 2, Name: 'Màu', Describe: 'Đỏ' },
  { id: 3, Name: 'Màu', Describe: 'Xanh dương' },
  { id: 4, Name: 'Màu', Describe: 'Xanh lá' },
  { id: 5, Name: 'Kích thước', Describe: 'Nhỏ' },
  { id: 6, Name: 'Kích thước', Describe: 'Trung bình' },
  { id: 7, Name: 'Kích thước', Describe: 'Lớn' },
  { id: 8, Name: 'Chất liệu', Describe: 'Vải cotton' },
  { id: 9, Name: 'Chất liệu', Describe: 'Da PU' },
  { id: 10, Name: 'Chất liệu', Describe: 'Kim loại' },
];

@Component({
  selector: 'app-product-form',
  standalone: false,
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.css'
})
export class ProductFormComponent {
  selectedImage: string | ArrayBuffer | null = null; // Lưu trữ URL của ảnh đã chọn
  imageFileName: string = "Chưa có hình ảnh nào được chọn"; // Tên file hiển thị
  fileInput: HTMLInputElement | null = null; // Tham chiếu đến input type="file"



  // Giá trị tìm kiếm
  searchSelectedValue: string = '';
  // Giá trị lọc
  filterSelectedValue: string = '';
  // Các tuỳ chọn lọc
  filterSelectedOptions = [
    { value: 'all', label: 'Tất cả' },
    { value: 'category1', label: 'Danh mục 1' },
    { value: 'category2', label: 'Danh mục 2' },
  ];
  // Danh sách đầy đủ các mục
  Selecteditems = [
    { value: 'item1', label: 'Mục 1', category: 'category1' },
    { value: 'item2', label: 'Mục 2', category: 'category2' },
    { value: 'item3', label: 'Mục 3', category: 'category1' },
    { value: 'item4', label: 'Mục 4', category: 'category2' },
    { value: 'item4', label: 'Mục 4', category: 'category2' },
    { value: 'item4', label: 'Mục 4', category: 'category2' },
    { value: 'item4', label: 'Mục 4', category: 'category2' },
  ];
  // Danh sách sau khi lọc
  filteredSelectedItems = [...this.Selecteditems];

  searchValue: string = '';
  filterValue: string = 'all'; // Giá trị mặc định cho bộ lọc
  filterOptions = [
    { value: 'all', label: 'Tất cả' },
    { value: 'Electronics', label: 'Thiết bị điện tử' },
    { value: 'Accessories', label: 'Phụ kiện' },
    { value: 'Gaming', label: 'Gaming' },
    { value: 'Office', label: 'Văn phòng' },
    { value: 'In Stock', label: 'Còn hàng' }, // Ví dụ lọc theo trạng thái
    { value: 'Out of Stock', label: 'Hết hàng' },
  ];

  // --- Dữ liệu và logic cho MatTable và Paginator ---
  // Các cột sẽ hiển thị trong bảng. 'select' cho checkbox
  displayedColumns: string[] = ['actions', 'id', 'Name', 'Describe'];

  // Nguồn dữ liệu cho MatTable. Ban đầu là ALL_ITEMS_DATA
  dataSource = new MatTableDataSource<ItemData>(ALL_ITEMS_DATA);
 selectedItems: ItemData[] = []; 

  // Model quản lý việc chọn hàng
  selection = new SelectionModel<ItemData>(true, []); // `true` cho phép chọn nhiều, `[]` là lựa chọn ban đầu trống

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit() {
    this.dataSource.data = ALL_ITEMS_DATA;
    // Không cần gọi filterList() ở đây vì filterList() sẽ tự động áp dụng filter/search
    // và cập nhật dataSource, sau đó paginator sẽ làm việc với dataSource.
    this.filterList(); // Áp dụng bộ lọc ban đầu khi component khởi tạo
  }

  ngAfterViewInit() {
    // Liên kết Paginator với DataSource
    this.dataSource.paginator = this.paginator;
  }


  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.fileInput = input; // Lưu lại tham chiếu đến input

    if (input.files && input.files[0]) {
      const file = input.files[0];
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
    // Đặt lại giá trị của input type="file" để có thể chọn lại cùng một file
    if (this.fileInput) {
      this.fileInput.value = '';
    }
  }

  // Phương thức cho việc submit form (ví dụ)
  onSubmit(): void {
    // Logic xử lý khi submit form
    console.log('Form submitted!');
    console.log('Selected Image:', this.selectedImage ? this.imageFileName : 'No image');
  }



  // Hàm lọc danh sách
  filterSelectedList() {
    this.filteredSelectedItems = this.Selecteditems.filter((item) => {
      const matchesSearch = item.label
        .toLowerCase()
        .includes(this.searchSelectedValue.toLowerCase());
      const matchesFilter =
        this.filterSelectedValue === 'all' || item.category === this.filterSelectedValue;
      return matchesSearch && matchesFilter;
    });
  }



  filterList() {
    this.dataSource.filterPredicate = (data: ItemData, filter: string) => {
      // Tìm kiếm theo 'name' hoặc 'Describe'
      const searchMatch = data.Name.toLowerCase().includes(this.searchValue.toLowerCase()) ||
        data.Describe.toLowerCase().includes(this.searchValue.toLowerCase());

      // Lọc theo 'name' (thuộc tính)
      const filterMatch = this.filterValue === 'all' ||
        data.Name === this.filterValue;

      return searchMatch && filterMatch;
    };

    // Kích hoạt lại bộ lọc mỗi khi searchValue hoặc filterValue thay đổi
    this.dataSource.filter = `${this.searchValue}-${this.filterValue}`;
  }

  isAllSelected() {
    const numSelected = this.selection.selected.length;
    // dataSource.filteredData chứa dữ liệu sau khi lọc (và phân trang đã được áp dụng tự động bởi paginator)
    const numRows = this.dataSource.filteredData.length;
    return numSelected === numRows;
  }

  /** Chọn tất cả các hàng nếu chúng chưa được chọn; ngược lại xóa lựa chọn. */
  toggleAllRows() {
    if (this.isAllSelected()) {
      this.selection.clear();
      return;
    }
    // Lựa chọn tất cả các hàng hiện đang hiển thị sau khi lọc và phân trang
    // dataSource.connect().value sẽ cung cấp cho bạn dữ liệu CỦA TRANG HIỆN TẠI
    this.dataSource.connect().value.forEach(row => this.selection.select(row));
  }

  /** Nhãn cho checkbox trên hàng được truyền vào */
  checkboxLabel(row?: ItemData): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${row.id}`;
  }
  toggleSelection(row: ItemData) {
    const index = this.selectedItems.findIndex(item => item.id === row.id);
    if (index > -1) {
      // Nếu đã chọn, bỏ chọn
      this.selectedItems.splice(index, 1);
    } else {
      // Nếu chưa chọn, thêm vào
      this.selectedItems.push(row);
    }
    console.log('Selected Items:', this.selectedItems);
  }
  isSelected(row: ItemData): boolean {
    return this.selectedItems.some(item => item.id === row.id);
  }
}
