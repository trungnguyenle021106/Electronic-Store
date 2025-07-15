import { Component } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter, Subject, takeUntil } from 'rxjs';
import { UserService } from '../../Service/User/user.service';
import { AuthService } from '../../Service/Auth/auth.service';

@Component({
  selector: 'app-account',
  standalone: false,
  templateUrl: './account.component.html',
  styleUrl: './account.component.css'
})
export class AccountComponent {
  isClickedOrder: boolean = false;
  isClickedInformation: boolean = false;
  isDisplayOrderDetail: boolean = false;
  isDisplayOverlay: boolean = false;
  orderID : number | null = null;

  private destroy$ = new Subject<void>();

  constructor(private router: Router, private userService: UserService, private authService : AuthService) { }

  ngOnInit(): void {
    // Lắng nghe sự kiện thay đổi route
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd), // Chỉ quan tâm đến NavigationEnd để lấy URL cuối cùng
      takeUntil(this.destroy$) // Hủy đăng ký khi component bị hủy
    ).subscribe(() => {
      this.updateNavigationFlags();
    });

    // Gọi lần đầu để thiết lập trạng thái khi component khởi tạo
    this.updateNavigationFlags();
  }


  private updateNavigationFlags(): void {
    const currentUrl = this.router.url; // Lấy URL hiện tại

    // Ví dụ: Giả sử các routes của bạn là '/order' và '/information'
    this.isClickedOrder = currentUrl.includes('/account/order');
    this.isClickedInformation = currentUrl.includes('/account/information');
  }
  
    OnClickLogout(): void {
    this.authService.logout().subscribe({
      next: () => {
      this.authService.setLoggedIn(false);
       this.router.navigate(['/']);
      },
      error: (err) => {
        console.error('Failed to fetch customer profile:', err);
      }
    });
  }


  ClickInformation(isClicked: boolean): void {
    this.isClickedInformation = isClicked;
  }

  ClickOrdder(isClicked: boolean): void {
    this.isClickedOrder = isClicked;
  }

  DisplayOrderDetail(orderID: number): void {
    this.orderID = orderID;
    this.isDisplayOrderDetail = true;
    this.DisplayOverlay(true);
  }

  DisplayOverlay(isDisplay: boolean): void {
    this.isDisplayOverlay = isDisplay;
    if (isDisplay == false) this.isDisplayOrderDetail = false;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
