import { Component } from '@angular/core';
interface Product {
  image: string;
  name: string;
  specs: string;
  price: string;
}
@Component({
  selector: 'app-category',
  standalone: false,
  templateUrl: './category.component.html',
  styleUrl: './category.component.css'
})
export class CategoryComponent {
  products: Product[] = [];
 ngOnInit(): void {
    const productTemplate: Product = {
      image: 'https://product.hstatic.net/200000722513/product/expertbook_b1_b1402_product_phot_c52b18232c29486283bb114a2faef66e.png',
      name: 'Laptop ASUS Zenbook 14 UX3405CA PZ204WS',
      specs: 'Ultra 7 155H • 32GB • 1TB • 14" 3K OLED 120Hz',
      price: '36.990.000đ'
    };

    // Tạo 10 bản sao của productTemplate
    this.products = Array.from({ length: 2}, () => productTemplate);
  }
}
