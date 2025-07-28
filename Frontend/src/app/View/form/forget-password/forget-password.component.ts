import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../../Service/Auth/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-forget-password',
  standalone: false,
  templateUrl: './forget-password.component.html',
  styleUrl: './forget-password.component.css'
})
export class ForgetPasswordComponent {
  step: number = 1; // Bước hiện tại
  emailForm: FormGroup;
  verificationForm: FormGroup;
  isResendDisabled: boolean = false; // Trạng thái nút Resend Code
  resendTimer: number = 0; // Bộ đếm thời gian

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private snackBar: MatSnackBar // Inject MatSnackBar
  ) {
    // Form nhập email
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });

    // Form xác thực mã và đổi mật khẩu
    this.verificationForm = this.fb.group({
      email: ['', Validators.required],
      code: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
    }, { validators: this.passwordsMatchValidator });
  }

  // Validator kiểm tra mật khẩu nhập lại
  passwordsMatchValidator(group: FormGroup) {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { notMatching: true };
  }

  // Gửi yêu cầu mã xác thực
  requestForgotPasswordCode() {
    if (this.emailForm.invalid) return;

    const email = this.emailForm.value.email;
    this.authService.requestForgotPasswordCode(email).subscribe({
      next: () => {
        this.openSnackBar('Mã xác thực đã được gửi đến email của bạn.', 'Đóng');
        this.step = 2; // Chuyển sang bước xác thực
        this.verificationForm.patchValue({ email }); // Gán email vào form xác thực
        this.startResendCooldown(); // Bắt đầu đếm ngược thời gian cho nút Resend Code
      },
      error: (err) => {
        this.openSnackBar(err.error.message || 'Đã xảy ra lỗi!', 'Đóng');
      },
    });
  }

  // Bắt đầu đếm ngược cho nút Resend Code
  startResendCooldown() {
    this.isResendDisabled = true; // Vô hiệu hóa nút Resend Code
    this.resendTimer = 60; // Thời gian chờ 60 giây

    const interval = setInterval(() => {
      this.resendTimer -= 1;

      if (this.resendTimer <= 0) {
        clearInterval(interval); // Dừng đếm khi hết thời gian
        this.isResendDisabled = false; // Kích hoạt lại nút Resend Code
      }
    }, 1000); // Cập nhật mỗi giây
  }

  // Quay lại bước nhập email
  goBackToEmailStep() {
    this.step = 1;
    this.verificationForm.reset(); // Reset form xác thực
    this.isResendDisabled = false; // Reset trạng thái nút Resend Code
  }

  // Kiểm tra mã xác thực và đổi mật khẩu
  verifyCodeAndResetPassword() {
    if (this.verificationForm.invalid) return;

    this.authService.checkVerificationCode(this.verificationForm.value).subscribe({
      next: () => {
        this.openSnackBar('Mật khẩu của bạn đã được đặt lại thành công!', 'Đóng');
        this.step = 3; // Hoàn tất
      },
      error: (err) => {
        this.openSnackBar(err.error.message || 'Mã xác thực không hợp lệ!', 'Đóng');
      },
    });
  }

  // Gửi lại mã xác thực
  resendVerificationCode() {
    const email = this.emailForm.value.email;
    this.authService.resendForgotPasswordCode(email).subscribe({
      next: () => {
        this.openSnackBar('Mã xác thực đã được gửi lại!', 'Đóng');
        this.startResendCooldown(); // Bắt đầu lại thời gian chờ
      },
      error: (err) => {
        this.openSnackBar(err.error.message || 'Không thể gửi lại mã xác thực!', 'Đóng');
      },
    });
  }

  // Mở Snackbar
  openSnackBar(message: string, action: string) {
    this.snackBar.open(message, action, {
      duration: 3000, // Hiển thị trong 3 giây
      horizontalPosition: 'center',
      verticalPosition: 'top', // Vị trí hiển thị của Snackbar
    });
  }
}
