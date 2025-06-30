import { Component } from '@angular/core';

import { ProductProperty } from '../../Model/Product/ProductProperty';
import { PropertyService } from '../../Service/Property/property.service';


@Component({
  selector: 'app-property',
  standalone: false,
  templateUrl: './property.component.html',
  styleUrl: './property.component.css'
})
export class PropertyComponent {
  isCreatePropertyDisplayed: boolean = false;
  isOverlayDisplayed: boolean = false;
  productProperties: ProductProperty[] = [];

  constructor(private productProperty: PropertyService) { }

  ngOnInit(): void {
    this.getAllProductProperties();
  }

  DisplayOverlay(isDisplayed: boolean): void {
    this.isOverlayDisplayed = isDisplayed;
  }

  DisplayCreateProperty(isDisplayed: boolean): void {
    this.isCreatePropertyDisplayed = isDisplayed;
  }

  getAllProductProperties(): void {
    this.productProperty.getAllProductProperties().subscribe({
      next: (data) => {
        this.productProperties = data;
        console.log('Dữ liệu Product Properties:', data); // <-- SỬA LẠI THÀNH data
      },
      error: (error) => {
        console.error('Lỗi khi lấy dữ liệu Product Properties:', error);
        // Xử lý lỗi ở đây (ví dụ: hiển thị thông báo lỗi cho người dùng)
      }
    });
  }
}
