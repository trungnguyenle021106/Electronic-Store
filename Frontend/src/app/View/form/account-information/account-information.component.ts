import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CustomerInformation } from '../../../Model/User/CustomerInformation';
import { UserService } from '../../../Service/User/user.service';

@Component({
  selector: 'app-account-information',
  standalone: false,
  templateUrl: './account-information.component.html',
  styleUrl: './account-information.component.css'
})
export class AccountInformationComponent {
  accountInfoForm!: FormGroup;
  customerInfo: CustomerInformation = {
    email: '',
    name: '',
    phone: '',
    address: '',
    gender: ''
  };

  constructor(private fb: FormBuilder, private userSerice: UserService) {
  }

  ngOnInit(): void {
    this.accountInfoForm = this.fb.group({
      name: ['', Validators.required], // Họ Tên - bắt buộc
      gender: ['male', Validators.required], // Giới tính - bắt buộc
      phone: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]], // Số điện thoại - bắt buộc, đúng định dạng 10 số
      email: [], // Email - chỉ đọc
      address: ['', Validators.required,], // Địa chỉ - bắt buộc
    });

    this.userSerice.getMyProfile().subscribe({
      next: (response: CustomerInformation) => { // Đảm bảo kiểu dữ liệu response khớp
        this.customerInfo = response;

        this.accountInfoForm.patchValue({
          email: this.customerInfo.email,
          name: this.customerInfo.name,
          address: this.customerInfo.address,
          gender: this.customerInfo.gender,
          phone: this.customerInfo.phone
        });

      },
      error: (error) => {
        console.error("Lỗi khi lấy thông tin khách hàng:", error.error?.message || error.message);
        // Xử lý lỗi, ví dụ: hiển thị thông báo cho người dùng
      }
    });
  }


  hasError(controlName: string, errorCode: string): boolean {
    const control = this.accountInfoForm.get(controlName);
    return control?.hasError(errorCode) && (control.dirty || control.touched) || false;
  }

  onSubmit(): void {
    if (this.accountInfoForm.valid) {
      console.log('Form submitted:', this.accountInfoForm.getRawValue());
    } else {
      console.log('Form is invalid');
      this.accountInfoForm.markAllAsTouched(); // Đánh dấu tất cả các control là "touched" để hiện lỗi
    }
  }
}
