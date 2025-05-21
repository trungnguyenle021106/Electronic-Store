import { Component } from '@angular/core';

@Component({
  selector: 'app-navigation',
  standalone: false,
  templateUrl: './navigation.component.html',
  styleUrl: './navigation.component.css'
})
export class NavigationComponent {
  isClickedCategory: boolean = false;
  isClickedRegister: boolean = false;
  isClickedLogin: boolean = false;
  isDisplayOverlay: boolean = false;

  ClickCategory(isClicked: boolean): void {
    this.DisplayOverlay(true);
    this.isClickedCategory = isClicked;
  }

  ClickRegister(isClicked: boolean): void {
    this.DisplayOverlay(true);
    this.isClickedRegister = isClicked;
  }

  ClickLogin(isClicked: boolean): void {
    this.DisplayOverlay(true);
    this.isClickedLogin = isClicked;
  }

  DisplayOverlay(isDisplay: boolean): void {
    this.isDisplayOverlay = isDisplay;
    if (this.isClickedCategory == true) {
      this.isClickedCategory = false;
    }
    else if (this.isClickedRegister == true) {
      this.isClickedRegister = false;
    }
    else if (this.isClickedLogin == true) {
      this.isClickedLogin = false;
    }
  }
}
