import { Component } from '@angular/core';
import { AuthService } from '../../Service/Auth/auth.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-navigation',
  standalone: false,
  templateUrl: './navigation.component.html',
  styleUrl: './navigation.component.css'
})
export class NavigationComponent {
  constructor(private authService: AuthService, private router: Router) {

  }

  OnClickLogOut() {
    this.authService.logout().subscribe({
      next: (response) => {
        // Xử lý khi đăng xuất thành công
        console.log('Đăng xuất thành công!', response);
        this.authService.setLoggedIn(false);
        this.router.navigateByUrl('/login', { replaceUrl: true });
      },
      error: (error: HttpErrorResponse) => { // Ép kiểu 'error' thành HttpErrorResponse
        console.error('Lỗi khi đăng xuất:', error); // Log toàn bộ đối tượng lỗi để debug

        let errorMessage = 'Đăng xuất thất bại. Vui lòng thử lại sau.'; // Thông báo lỗi mặc định
        if (error.status === 500) {
          console.error('Status Code:', error.status); // Sẽ là 500
          console.error('Error Title:', error.error.title); // Sẽ là "Internal Server Error"
          console.error('Error Detail:', error.error.detail); // Sẽ là giá trị của result.ErrorMessage từ backend
        }
        if (error.error instanceof ErrorEvent) {
          // Lỗi phía client-side hoặc mạng
          errorMessage = `Lỗi mạng: ${error.error.message}`;
        } else {
          // Lỗi từ server (Problem Details hoặc đối tượng lỗi khác)
          // Kiểm tra xem lỗi có phải là Problem Details không
          if (error.error && typeof error.error === 'object' && error.error.title && error.error.detail) {
            // Đây là Problem Details
            errorMessage = error.error.detail; // Lấy chi tiết lỗi từ 'detail'
          } else if (error.error && typeof error.error === 'object' && error.error.message) {
            // Đây là trường hợp BadRequest bạn trả về { message: ... }
            errorMessage = error.error.message;
          } else {
            // Các loại lỗi server không xác định khác
            errorMessage = `Lỗi từ máy chủ: Mã trạng thái ${error.status}`;
          }
        }

        alert(errorMessage); // Hiển thị thông báo lỗi đã được xử lý
      }
    });
  }
}
