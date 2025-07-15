import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-account-order',
  standalone: false,
  templateUrl: './account-order.component.html',
  styleUrl: './account-order.component.css'
})
export class AccountOrderComponent {
  isEmptyOrder: boolean = false;

  @Output() isDisplayOrderDetailEmitter = new EventEmitter<number>();

  DisplayOrderDetail(orderID : number): void {
    this.isDisplayOrderDetailEmitter.emit(orderID);
  }

}
