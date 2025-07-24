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
    Email: '',
    Name: '',
    Phone: '',
    Address: '',
    Gender: ''
  };

  constructor(private fb: FormBuilder, private userSerice: UserService) {
  }

  ngOnInit(): void {
    this.accountInfoForm = this.fb.group({
      Name: ['', Validators.required], // Họ Tên - bắt buộc
      Gender: ['male', Validators.required], // Giới tính - bắt buộc
      Phone: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]], // Số điện thoại - bắt buộc, đúng định dạng 10 số
      Email: [], // Email - chỉ đọc
      Address: ['', Validators.required,], // Địa chỉ - bắt buộc
    });

    this.userSerice.getMyProfile().subscribe({
      next: (response: CustomerInformation) => { // Đảm bảo kiểu dữ liệu response khớp
        this.customerInfo = response;

        this.accountInfoForm.patchValue({
          Email: this.customerInfo.Email,
          Name: this.customerInfo.Name,
          Address: this.customerInfo.Address,
          Gender: this.customerInfo.Gender,
          Phone: this.customerInfo.Phone
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
      
    } else {
      console.log('Form is invalid');
      this.accountInfoForm.markAllAsTouched(); // Đánh dấu tất cả các control là "touched" để hiện lỗi
    }
  }
}
