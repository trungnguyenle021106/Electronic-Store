import { Component } from '@angular/core';

@Component({
  selector: 'app-create-product',
  standalone: false,
  templateUrl: './create-product.component.html',
  styleUrl: './create-product.component.css'
})
export class CreateProductComponent {
  selectedImage: string | ArrayBuffer | null = null; // Lưu trữ URL của ảnh đã chọn
  imageFileName: string = "Chưa có hình ảnh nào được chọn"; // Tên file hiển thị
  fileInput: HTMLInputElement | null = null; // Tham chiếu đến input type="file"

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
}
