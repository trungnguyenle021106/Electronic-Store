import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { UserService } from '../../Service/User/user.service';
import { LoginRequest } from '../../Model/Request/LoginRequest';
import { AuthService } from '../../Service/Auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginData: LoginRequest = {
    Email: '',
    Password: ''
  };

  constructor(
    private authService: AuthService,
  ) { }

  onSubmit(form: NgForm): void {
    if (form.valid) {
      console.log('Dữ liệu form hợp lệ:', this.loginData);

      this.authService.login(this.loginData).subscribe({
        next: (response) => {
          window.location.href = response;
        },
        error: (error) => {
          console.error('Đăng nhập thất bại:', error);
          // Xử lý lỗi đăng nhập (hiển thị thông báo lỗi cho người dùng)
          alert('Đăng nhập thất bại. Vui lòng kiểm tra lại email và mật khẩu.');
        }
      });
    } else {
      console.log('Form không hợp lệ. Vui lòng điền đầy đủ thông tin.');
      alert('Vui lòng điền đầy đủ và hợp lệ các trường.');
    }
  }

}
