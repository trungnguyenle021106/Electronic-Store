
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { HttpClient } from '@angular/common/http';
import { ProductProperty } from '../../Model/Product/ProductProperty';


@Injectable({
  providedIn: 'root'
})
export class PropertyService {
  private baseApiUrl = 'http://localhost:5293';
  // private apiUrl = 'http://your-api-domain.com/productProperties'; // Đặt URL API của bạn tại đây

  constructor(private http: HttpClient) { }

  // Phương thức để gửi POST request
  createProductProperty(productProperty: ProductProperty): Observable<any> {
    let apiUrl: string = `${this.baseApiUrl}/productProperties`;
    return this.http.post<any>(apiUrl, productProperty, { withCredentials: true });
  }

  getAllProductProperties(): Observable<ProductProperty[]> {

    let apiUrl: string = `${this.baseApiUrl}/gateway/productsapi/productProperties`;
    return this.http.get<ProductProperty[]>(apiUrl, { withCredentials: true });
  }
}
