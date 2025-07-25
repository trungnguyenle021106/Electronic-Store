import { Component, Inject, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

interface DialogData {
  actionName: string,
  onConfirm: () => void; // Phương thức callback không có tham số và không trả về gì
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: false,
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.css'
})
export class ConfirmDialogComponent {
  readonly dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);
  // Tiêm MAT_DIALOG_DATA để nhận dữ liệu từ component cha
  constructor(@Inject(MAT_DIALOG_DATA) public data: DialogData) { }

  // Phương thức này được gọi khi người dùng nhấn nút "Đồng ý"
  onConfirmClick(): void {
    // ✨ GỌI PHƯƠNG THỨC ĐƯỢC TRUYỀN TỪ COMPONENT CHA ✨
    if (this.data && this.data.onConfirm) {
      this.data.onConfirm();
    }
    // Đóng dialog sau khi thực hiện hành động
    this.dialogRef.close();
  }

  // Phương thức được gọi khi người dùng nhấn nút "Hủy"
  onCancelClick(): void {
    this.dialogRef.close(); // Đóng dialog mà không truyền giá trị
  }
}
