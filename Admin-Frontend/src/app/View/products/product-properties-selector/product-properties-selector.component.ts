import { Component } from '@angular/core';

@Component({
  selector: 'app-product-properties-selector',
  standalone: false,
  templateUrl: './product-properties-selector.component.html',
  styleUrl: './product-properties-selector.component.css'
})
export class ProductPropertiesSelectorComponent {
  allAttributes: string[] = [
    "Màu sắc", "Kích thước", "Chất liệu", "Xuất xứ", "Hạn sử dụng",
    "Trọng lượng", "Kiểu dáng", "Công suất", "Dung tích", "Số lượng",
    "Thương hiệu", "Bảo hành"
  ];

  currentAttributes: string[] = ["Màu sắc", "Kích thước", "Chất liệu"];
  availableAttributes: string[] = [];

  ngOnInit(): void {
    this.updateAttributes(); // Gọi khi component được khởi tạo
  }

  private updateAttributes(): void {
    // Lọc ra các thuộc tính có sẵn dựa trên các thuộc tính hiện có
    this.availableAttributes = this.allAttributes.filter(
      attr => !this.currentAttributes.includes(attr)
    ).sort(); // Sắp xếp lại để hiển thị đẹp hơn
  }

  addAvailableAttribute(attribute: string): void {
    // Thêm vào currentAttributes
    this.currentAttributes.push(attribute);
    this.currentAttributes.sort(); // Sắp xếp lại

    // Cập nhật lại availableAttributes
    this.updateAttributes();
  }

  removeCurrentAttribute(attribute: string): void {
    // Tìm index của thuộc tính trong currentAttributes và xóa nó
    const index = this.currentAttributes.indexOf(attribute);
    if (index > -1) {
      this.currentAttributes.splice(index, 1);
    }

    // Cập nhật lại availableAttributes
    this.updateAttributes();
  }
}
