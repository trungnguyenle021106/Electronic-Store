import { Component } from '@angular/core';

@Component({
  selector: 'app-account',
  standalone: false,
  templateUrl: './account.component.html',
  styleUrl: './account.component.css'
})
export class AccountComponent {
  isClickedOrder: boolean = true;
  isClickedInformation: boolean = false;
  isEmptyOrder: boolean = false;
  isDisplayOrderDetail: boolean = false;
  isDisplayOverlay: boolean = false;

  ClickInformation(isClicked: boolean): void {
    this.isClickedInformation = isClicked;
  }

  ClickOrdder(isClicked: boolean): void {
    this.isClickedOrder = isClicked;
  }

  DisplayOrderDetail(orderID: number): void {
    this.isDisplayOrderDetail = true;
    this.DisplayOverlay(true);
  }

  DisplayOverlay(isDisplay: boolean): void {
    this.isDisplayOverlay = isDisplay;
    if (isDisplay == false) this.isDisplayOrderDetail = false;
  }
}
