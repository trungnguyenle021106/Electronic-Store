import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../../Service/Auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  @Output() statusLoginEmitter = new EventEmitter<boolean>();
  loginForm: FormGroup;
  serverError: string | null = null;

  constructor(private fb: FormBuilder, private authService: AuthService) {
    // Tạo form với kiểm tra validation
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      const formData = this.loginForm.value;

      this.serverError = null; // Reset lỗi server trước khi gửi

      this.authService.login(formData).subscribe({
        next: (response) => {
          window.location.href = response;
        },
        error: (error) => {
          this.serverError = error.error.message || 'Đăng nhập thất bại. Vui lòng thử lại.';
        },
      })
    }
  }

  CloseLoginForm(): void {
    this.statusLoginEmitter.emit(false);
  }
}
