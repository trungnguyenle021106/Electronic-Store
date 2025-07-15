import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-orderdetail',
  standalone: false,
  templateUrl: './orderdetail.component.html',
  styleUrl: './orderdetail.component.css'
})
export class OrderdetailComponent {

  @Input() receivedOrderId: number | null = null;
}
