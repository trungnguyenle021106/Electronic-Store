import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable, Injector } from "@angular/core";
import { AuthService } from "../Service/Auth/auth.service";
import { catchError, filter, Observable, switchMap, take, throwError } from "rxjs";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(
    private injector: Injector // Inject AuthService
  ) { }


  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        // Kiểm tra lỗi 401 VÀ đảm bảo không phải yêu cầu refresh token
        if (error.status === 401 && !request.url.includes('/auth/tokens/refresh')) {
          if (error.status === 401 && !request.url.includes('/auth/tokens/refresh')) {
            console.warn('AuthInterceptor: Received 401 Unauthorized for a protected route. Attempting token refresh...');
            return this.handle401Error(request, next, error);
          }
          // =========================================================================
        }

        // Nếu không phải lỗi 401, hoặc là 401 từ chính yêu cầu refresh token,
        // thì ném lỗi bình thường để các phần khác của ứng dụng xử lý.
        return throwError(() => error);
      })
    );
  }

  private handle401Error(request: HttpRequest<unknown>, next: HttpHandler, error: HttpErrorResponse): Observable<HttpEvent<unknown>> {
    const authService = this.injector.get(AuthService);
    if (!authService.isRefreshing) {
      // console.log('AuthInterceptor: No refresh in progress. Initiating refresh...');

      return authService.refreshToken().pipe(
        switchMap((success: boolean) => {
          // console.log('AuthInterceptor: Token refreshed successfully (via HTTP-only cookie). Retrying original request.');
          return next.handle(request);
        }),
        catchError((refreshError) => {
          console.error('AuthInterceptor: Refresh token failed. Logging out.', refreshError);
          return throwError(() => refreshError);
        })
      );
    } else {
      // console.log('AuthInterceptor: Refresh already in progress. Queuing original request.');
      return authService.refreshTokenSubject.pipe(
        filter(success => success === true),
        take(1),
        switchMap((success: boolean) => {
          return next.handle(request);
        }),
        catchError(error => {
          console.error('AuthInterceptor: Queued request failed because refresh token failed.', error);
          return throwError(() => error);
        })
      );
    }
  }
}