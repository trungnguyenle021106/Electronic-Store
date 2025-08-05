import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CustomerInformation } from '../../../Model/User/CustomerInformation';
import { UserService } from '../../../Service/User/user.service';
import { ConfirmDialogComponent } from '../../dialogs/confirm-dialog/confirm-dialog.component';
import { ErrorDialogComponent } from '../../dialogs/error-dialog/error-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { Customer } from '../../../Model/User/Customer';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-account-information',
  standalone: false,
  templateUrl: './account-information.component.html',
  styleUrl: './account-information.component.css'
})
export class AccountInformationComponent {
  accountInfoForm!: FormGroup;
  customerInfo: CustomerInformation = {
    ID: 0,
    Email: '',
    Name: '',
    Phone: '',
    Address: '',
    Gender: ''
  };

  readonly dialog = inject(MatDialog);

  constructor(private fb: FormBuilder, private userSerice: UserService, private snackBar: MatSnackBar) {
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
      this.openConfirmDialog("300ms", "150ms", "cập nhật thông tin");
    } else {
      this.accountInfoForm.markAllAsTouched(); // Đánh dấu tất cả các control là "touched" để hiện lỗi
    }
  }

  UpdateInformation() {
    const newCustomer: Customer = {
      ID: this.customerInfo.ID, // Giữ nguyên ID từ customerInfo hiện có
      Name: this.accountInfoForm.get('Name')?.value, // Lấy giá trị của trường 'Name'
      Phone: this.accountInfoForm.get('Phone')?.value, // Lấy giá trị của trường 'Phone'
      Address: this.accountInfoForm.get('Address')?.value, // Lấy giá trị của trường 'Address'
      Gender: this.accountInfoForm.get('Gender')?.value, // Lấy giá trị của trường 'Gender'
    };
    this.userSerice.UpdateCustomerInformation(this.customerInfo.ID, newCustomer).subscribe({
      next: (response) => {

        this.accountInfoForm.patchValue({
          Name: response.Name,
          Phone: response.Phone,
          Address: response.Address,
          Gender: response.Gender,
        });

        this.snackBar.open('Cập nhật thông tin thành công!', 'Đóng', {
          duration: 3000,
          horizontalPosition: 'end',  // Bên phải
          verticalPosition: 'top',    // Phía trên
          panelClass: ['success-snackbar']
        });
      },
      error: (error) => {
        this.openErrorDialog("300ms", "150ms", "Lỗi cập nhật thông tin", "Cập nhật thông tin thất bại, vui lòng thử lại sau")
        console.log(error);
      }
    })
  }

  openConfirmDialog(enterAnimationDuration: string, exitAnimationDuration: string, actionName: string): void {
    this.dialog.open(ConfirmDialogComponent, {
      width: '300px', // Tăng width để dễ nhìn hơn
      enterAnimationDuration,
      exitAnimationDuration,
      // ✨ TRUYỀN PHƯƠNG THỨC VÀO ĐÂY QUA THUỘC TÍNH 'data' ✨
      data: {
        actionName: actionName,
        onConfirm: () => this.UpdateInformation()
      }
    });
  }

  openErrorDialog(enterAnimationDuration: string, exitAnimationDuration: string, errorTitle: string, errorMessage: string): void {
    this.dialog.open(ErrorDialogComponent, {
      width: '300px', // Kích thước phù hợp với dialog lỗi
      enterAnimationDuration,
      exitAnimationDuration,
      // Truyền tiêu đề và thông báo lỗi vào dialog
      data: {
        title: errorTitle,
        message: errorMessage
      },
      disableClose: true, // Thường là lỗi thì không cho click ra ngoài đóng
      hasBackdrop: true, // Luôn có backdrop
    });
  }
}
