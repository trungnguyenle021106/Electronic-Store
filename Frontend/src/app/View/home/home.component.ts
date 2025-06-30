import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  standalone: false,
})
export class HomeComponent {
  showSubMenu = false;

  toggleSubMenu(status: boolean, category : string): void {
    this.showSubMenu = status;
  }
}