import { Component, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductService } from '../../Service/Product/product.service';
import { Product } from '../../Model/Product/Product';
import { ProductProperty } from '../../Model/Product/ProductProperty';
import { CartService } from '../../Service/Cart/cart.service';

@Component({
  selector: 'app-product-detail',
  standalone: false,
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css',
})
export class ProductDetailComponent {

  productID: number = 0;
  product: Product | null = null;
  productProperties: ProductProperty[] = [];

  constructor(private route: ActivatedRoute, private productService: ProductService, private cartService: CartService) {

  }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.productID = parseInt(idParam, 10);
        this.loadProductDetail();
        this.loadProductPropertiesDetail();
      }
    });
  }

  private loadProductDetail() {
    this.productService.getProductByID(this.productID).subscribe({
      next: (response) => {
        this.product = response;
      },
      error: (error) => {
        console.log(error);
      }
    })
  }

  private loadProductPropertiesDetail() {
    this.productService.getAllPropertiesOfProduct(this.productID).subscribe({
      next: (response) => {
        this.productProperties = response;
      },
      error: (error) => {
        console.log(error);
      }
    })
  }

  OnclickBuy() {
    if (this.product) {
      this.cartService.addToCart(this.product, 1);
    }
  }
}
