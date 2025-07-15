import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Product } from '../../Model/Product/Product';
import { Observable } from 'rxjs';
import { ProductDTO } from '../../Model/Product/DTO/Response/ProductDTO';
import { PagedResult } from '../../Model/Product/DTO/Response/PagedResult';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private baseApiUrl = 'http://localhost:5293';
  constructor(private http: HttpClient) { }

  createProduct(product: Product): Observable<Product> {
    let apiUrl: string = `${this.baseApiUrl}/gateway/productsapi/products`;
    return this.http.post<Product>(apiUrl, product, { withCredentials: true });
  }

  getPagedProducts(currentPage : number = 1, pageSize : number = 10): Observable<PagedResult<ProductDTO>> {
    let apiUrl: string = `${this.baseApiUrl}/gateway/productsapi/products?page=${currentPage}&pageSize=${pageSize}`;
    return this.http.get<PagedResult<ProductDTO>>(apiUrl, { withCredentials: true });
  }
}
