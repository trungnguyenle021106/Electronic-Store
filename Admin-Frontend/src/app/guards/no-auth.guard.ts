// src/app/guards/no-auth.guard.ts
import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { catchError, map, take, tap } from 'rxjs/operators'; // Thêm take để unsubscribe sau khi nhận được giá trị đầu tiên
import { AuthService } from '../Service/Auth/auth.service';
import { of } from 'rxjs';

export const noAuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Lắng nghe trạng thái đăng nhập từ BehaviorSubject
  // Sử dụng 'take(1)' để chỉ lấy giá trị hiện tại và hoàn thành Observable,
  // tránh tạo ra các subscription không cần thiết.
  return authService.checkStatus().pipe(
    map(currentUserRole => {
      // Nếu currentUserRole KHÔNG phải là null (nghĩa là người dùng ĐÃ đăng nhập)
      if (currentUserRole) {
        // console.log('Người dùng đã đăng nhập. Ngăn truy cập vào trang không yêu cầu xác thực và chuyển hướng đến trang chính.');
        // Trả về UrlTree để Router tự động điều hướng đến trang chính
        return router.createUrlTree(['/']);
      }

      // Nếu currentUserRole là null (nghĩa là người dùng CHƯA đăng nhập)
      // console.log('Người dùng chưa đăng nhập. Cho phép truy cập vào trang không yêu cầu xác thực.');
      return true; // Cho phép truy cập
    }),
    catchError(error => {
      console.error('Lỗi trong noAuthGuard khi kiểm tra API trạng thái xác thực:', error);
      // Nếu có lỗi API (ví dụ: không kết nối được server), chúng ta coi như người dùng chưa đăng nhập
      // và cho phép họ truy cập trang login/register.
      // Bạn có thể cân nhắc chuyển hướng đến một trang lỗi chung nếu muốn xử lý lỗi nghiêm trọng hơn.
      return of(true); // Trả về Observable<true> để cho phép truy cập
    })
  );
};