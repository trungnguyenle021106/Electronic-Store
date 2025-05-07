import {Component} from '@angular/core';
import {HomeComponent} from './home/home.component';
@Component({
  selector: 'app-root',
  standalone : false,
  templateUrl:'./app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  title = 'homes';
  isSidebarOpen = false;

  categories = [
    'Laptop', 'Laptop Gaming', 'PC GVN',
    'Main, CPU, VGA', 'Case, Nguồn, Tản',
    'Ổ cứng, RAM, Thẻ nhớ', 'Loa, Micro, Webcam',
    'Màn hình', 'Bàn phím', 'Chuột + Lót chuột',
    'Tai Nghe', 'Ghế - Bàn', 'Phần mềm, mạng',
    'Handheld, Console', 'Phụ kiện (Hub, sạc...)',
    'Dịch vụ và thông tin khác'
  ];

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }
}