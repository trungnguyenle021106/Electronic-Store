import { Component } from '@angular/core';
import { AuthService } from '../../Service/Auth/auth.service';
import { filter, Observable, Subject, switchMap, takeUntil } from 'rxjs';
import { UserService } from '../../Service/User/user.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navigation',
  standalone: false,
  templateUrl: './navigation.component.html',
  styleUrl: './navigation.component.css'
})
export class NavigationComponent {
  isClickedCategory: boolean = false;
  isClickedRegister: boolean = false;
  isClickedLogin: boolean = false;
  isDisplayOverlay: boolean = false;

  isLoggedIn: Observable<boolean>;

  customerName: string = "";

  private destroy$ = new Subject<void>();

  constructor(private authService: AuthService, private userService: UserService, private router : Router) {
    this.isLoggedIn = this.authService.isLoggedIn;
  }

  ngOnInit() {
    this.isLoggedIn
      .pipe(
        takeUntil(this.destroy$), // Đảm bảo hủy subscription khi component bị hủy
        filter(isLoggedIn => isLoggedIn), // Chỉ tiếp tục nếu isLoggedIn là true
        switchMap(() => {
          // Nếu isLoggedIn là true, gọi phương thức getMyProfile() từ AuthService
          console.log('User is logged in. Fetching customer profile from AuthService...');
          return this.userService.getMyProfile(); // <-- Gọi phương thức của AuthService
        })
      )
      .subscribe({
        next: (info) => {
          this.customerName = info.name;
        },
        error: (err) => {
          console.error('Failed to fetch customer profile:', err);
          this.customerName = "";
        }
      });
  }

  OnClickLogout(): void {
    this.authService.logout().subscribe({
      next: () => {
      this.authService.setLoggedIn(false);
      },
      error: (err) => {
        console.error('Failed to fetch customer profile:', err);
      }
    });
  }

  ClickCategory(isClicked: boolean): void {
    this.DisplayOverlay(true);
    this.isClickedCategory = isClicked;
  }

  ClickRegister(isClicked: boolean): void {
    this.DisplayOverlay(true);
    this.isClickedRegister = isClicked;
  }

  ClickLogin(isClicked: boolean): void {
    this.DisplayOverlay(true);
    this.isClickedLogin = isClicked;
  }

  DisplayOverlay(isDisplay: boolean): void {
    this.isDisplayOverlay = isDisplay;
    if (this.isClickedCategory == true) {
      this.isClickedCategory = false;
    }
    else if (this.isClickedRegister == true) {
      this.isClickedRegister = false;
    }
    else if (this.isClickedLogin == true) {
      this.isClickedLogin = false;
    }
  }

  ngOnDestroy() {
    this.destroy$.next(); // Phát ra tín hiệu
    this.destroy$.complete(); // Hoàn thành Subject để giải phóng tài nguyên
  }
}
