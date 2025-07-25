import { Component, Inject } from '@angular/core';
import { UserService } from '../../../Service/User/user.service';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CustomerInformation } from '../../../Model/User/CustomerInformation';

@Component({
  selector: 'app-customer',
  standalone: false,
  templateUrl: './customer.component.html',
  styleUrl: './customer.component.css'
})
export class CustomerComponent {

  customerInformation: CustomerInformation | null = null;

  constructor(
    public dialogRef: MatDialogRef<CustomerComponent>,
    @Inject(MAT_DIALOG_DATA) public data: number,
    private userService: UserService
  ) { }


  ngOnInit() {
    this.loadCustomer();
  }

  loadCustomer() {
    this.userService.getCustomerInformationByAccountID(this.data).subscribe({
      next: (response) => {
        this.customerInformation = response;
        console.log(response)
      }, error: (error) => {
        console.log(error);
      }
    })
  }

  onClose(): void {
    this.dialogRef.close(); // Đóng dialog
  }
}
