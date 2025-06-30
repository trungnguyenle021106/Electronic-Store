import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core'; // <--- Import 'inject'
import { tap, map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { AuthService } from '../Service/Auth/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  // Lấy instance của AuthService và Router bằng hàm inject()
  const authService = inject(AuthService);
  const router = inject(Router);

  // Gọi API để kiểm tra trạng thái đăng nhập
  // Phương thức checkLoginStatus() của bạn trả về Observable<boolean> rất phù hợp
  return authService.checkStatus().pipe(
    map(currentUserRole => {
      // Nếu API trả về null, nghĩa là người dùng chưa đăng nhập hoặc phiên đã hết hạn
      if (!currentUserRole) {
        // console.log('Người dùng chưa đăng nhập hoặc phiên hết hạn. Chuyển hướng đến /login');
        return router.createUrlTree(['/login']); // Trả về UrlTree để chuyển hướng
      }


      if (currentUserRole === 'Customer') {
        // Nếu là tài khoản Customer, điều hướng về trang chủ bằng window.location.href (tải lại toàn bộ trang)
        window.location.href = 'http://localhost:4200'; // **Đảm bảo đúng cổng và giao thức**
        return false; // Trả về false để ngăn Angular Router tiếp tục điều hướng nội bộ
      }
      return true;
    }),
    catchError(error => {
      // Xử lý lỗi từ API (ví dụ: mất kết nối mạng, server không phản hồi)
      console.error('Lỗi trong authGuard khi gọi API kiểm tra trạng thái xác thực:', error);
      // Trong trường hợp có lỗi nghiêm trọng khi gọi API, chúng ta coi như người dùng chưa đăng nhập
      return of(router.createUrlTree(['/login'])); // Trả về Observable<UrlTree>
    })
  );

};