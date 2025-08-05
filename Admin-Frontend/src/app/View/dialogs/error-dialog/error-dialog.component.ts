import { Component, inject, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
export interface ErrorDialogData {
  title: string;
  message: string;
}
@Component({
  selector: 'app-error-dialog',
  standalone: false,
  templateUrl: './error-dialog.component.html',
  styleUrl: './error-dialog.component.css'
})
export class ErrorDialogComponent {
  readonly dialogRef = inject(MatDialogRef<ErrorDialogComponent>);

  // Tiêm MAT_DIALOG_DATA để nhận tiêu đề và thông báo lỗi
  constructor(@Inject(MAT_DIALOG_DATA) public data: ErrorDialogData) { }

  // Phương thức này sẽ được gọi khi nhấn nút "Đóng"
  onCloseClick(): void {
    this.dialogRef.close(); // Đóng dialog
  }
}
