import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, filter, Observable, of, shareReplay, take, tap, throwError } from 'rxjs';

import { Router } from '@angular/router';
import { LoginRequest } from '../../Model/Requests/LoginRequest';
import { SignUpRequest } from '../../Model/Requests/SignUpRequest';
import { VerifyRequest } from '../../Model/Requests/VerifyRequest';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseApiUrl = 'http://localhost:5293/gateway/user-apis';

  private _isLoggedIn = new BehaviorSubject<boolean>(false);
  public isLoggedIn = this._isLoggedIn.asObservable();

  private currentUserRoleSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  public currentUserRole: Observable<string | null> = this.currentUserRoleSubject.asObservable();

  // Biến cờ và Subject để quản lý quá trình refresh token
  public isRefreshing = false;

  // BehaviorSubject này sẽ phát ra token MỚI khi refresh thành công, hoặc lỗi khi refresh thất bại
  public refreshTokenSubject: BehaviorSubject<boolean | null> = new BehaviorSubject<boolean | null>(null);

  private authStatusCheckCached$: Observable<string | null>;

  constructor(private http: HttpClient, private router: Router) {
    this.authStatusCheckCached$ = this.performAuthStatusCheck().pipe(shareReplay(1));
    this.authStatusCheckCached$.subscribe({
      next: (response) => {
        // console.log('AuthService: Initial auth status check completed and cached.')
      },
      error: (error) => {
        console.error('AuthService: Initial auth status check failed:', error)
      },
    });
  }

  setLoggedIn(status: boolean): void {
    this._isLoggedIn.next(status); // Gọi .next() để cập nhật giá trị của BehaviorSubject
  }

  private performAuthStatusCheck(): Observable<string | null> {
    return this.http.get<string>(`${this.baseApiUrl}/auth/status`, { withCredentials: true }).pipe(
      tap((roleResponse: string) => {
        // Nếu request thành công, nghĩa là người dùng đã đăng nhập và backend trả về vai trò
        this._isLoggedIn.next(true);
        this.currentUserRoleSubject.next(roleResponse); // Cập nhật vai trò
      }),
      catchError((error: HttpErrorResponse) => {
        // Nếu có lỗi (ví dụ: 401 Unauthorized), nghĩa là chưa đăng nhập hoặc token hết hạn
        console.warn('AuthService: User is NOT logged in or session expired.', error);
        this._isLoggedIn.next(false);
        this.currentUserRoleSubject.next(null); // Đặt vai trò về null
        return of(null); // Trả về null để cho biết không có vai trò
      })
    );
  }

  checkStatus(): Observable<string | null> {
    return this.authStatusCheckCached$;
  }

  login(loginRequest: LoginRequest): Observable<string> {
    let apiUrl: string = `${this.baseApiUrl}/auth/login`;
    return this.http.post<string>(apiUrl, loginRequest, { withCredentials: true });
  }

  logout(): Observable<void> {
    let apiUrl: string = `${this.baseApiUrl}/auth/logout/specific`;
    return this.http.post<void>(apiUrl, null, { withCredentials: true });
  }

  refreshToken(): Observable<boolean> {
    const apiUrl: string = `${this.baseApiUrl}/auth/tokens/refresh`;

    if (this.isRefreshing) {
      return this.refreshTokenSubject.asObservable().pipe(
        // Vẫn chờ giá trị 'true' từ refreshTokenSubject để tiếp tục
        filter(success => success === true),
        take(1)
      ) as Observable<boolean>;
    }

    this.isRefreshing = true;
    this.refreshTokenSubject.next(null); // Báo hiệu refresh đang diễn ra

    return this.http.post<any>(apiUrl, {}, { withCredentials: true }).pipe(
      tap(response => {
        // console.log('AuthService: Refresh Token API Response (HTTP-only cookies used).');
        this.setLoggedIn(true);
        this.isRefreshing = false; // Reset cờ khi thành công
        this.refreshTokenSubject.next(true); // Phát 'true' để báo hiệu thành công cho các request đang chờ
      }),
      catchError((error: HttpErrorResponse) => {
        console.error('AuthService: Refresh Token Failed. Logging out.', error);
        this.setLoggedIn(false);
        this.isRefreshing = false; // Reset cờ khi thất bại

        // ======================================================================================
        // ĐIỀU CHỈNH QUAN TRỌNG TẠI ĐÂY:
        // 1. Phát 'false' để báo hiệu thất bại cho các request đang chờ.
        //    (Các request đang chờ sẽ không tiếp tục vì filter(success => success === true) sẽ chặn 'false').
        this.refreshTokenSubject.next(false);
        // 2. Khởi tạo lại refreshTokenSubject để nó sẵn sàng cho lần refresh tiếp theo.
        //    Điều này ngăn chặn việc Subject bị ở trạng thái error vĩnh viễn.
        this.refreshTokenSubject = new BehaviorSubject<boolean | null>(null);
        // ======================================================================================

        // Ném lỗi lại để AuthInterceptor hoặc nơi gọi có thể bắt và xử lý thêm
        return throwError(() => error);
      })
    );
  }

  signUp(signUpRequest: SignUpRequest): Observable<void> {
    let apiUrl: string = `${this.baseApiUrl}/auth/sign-up`;
    return this.http.post<void>(apiUrl, signUpRequest, { withCredentials: true });
  }


   requestForgotPasswordCode(email: string): Observable<any> {
    return this.http.post(`${this.baseApiUrl}/accounts/forget-password`, JSON.stringify(email), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  // 2. Kiểm tra mã xác thực
  checkVerificationCode(request: VerifyRequest): Observable<any> {
    return this.http.post(`${this.baseApiUrl}/accounts/forget-password/check-verify-code`, request);
  }

  // 3. Gửi lại mã xác thực quên mật khẩu
  resendForgotPasswordCode(email: string): Observable<any> {
    return this.http.post(`${this.baseApiUrl}/accounts/forget-password/resend-code`, JSON.stringify(email), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}
