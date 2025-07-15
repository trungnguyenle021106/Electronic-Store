
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { HttpClient, HttpParams } from '@angular/common/http';
import { ProductProperty } from '../../Model/Product/ProductProperty';
import { PagedResult } from '../../Model/Product/DTO/Response/PagedResult';


@Injectable({
  providedIn: 'root'
})
export class PropertyService {
  private baseApiUrl = 'http://localhost:5293/gateway/product-apis';

  constructor(private http: HttpClient) { }

  // Phương thức để gửi POST request
  createProductProperty(listCreate: ProductProperty[]): Observable<any> {
    let apiUrl: string = `${this.baseApiUrl}/product-properties`;
    return this.http.post<any>(apiUrl, listCreate, { withCredentials: true });
  }


  getPagedProductProperties(
    currentPage: number = 1,
    pageSize: number = 10,
    searchText: string = '',
    filter: string = ''
  ): Observable<PagedResult<ProductProperty>> {

    // Khởi tạo HttpParams
    let params = new HttpParams()
      .set('page', currentPage.toString()) // Chuyển số sang chuỗi
      .set('pageSize', pageSize.toString()); // Chuyển số sang chuỗi

    // Thêm searchText vào params nếu nó có giá trị (không rỗng, không khoảng trắng)
    if (searchText && searchText.trim().length > 0) {
      params = params.set('searchText', searchText.trim()); // Sử dụng trim() để loại bỏ khoảng trắng thừa
    }

    // Thêm filter vào params nếu nó có giá trị (không rỗng, không khoảng trắng)
    if (filter && filter.trim().length > 0) {
      params = params.set('filter', filter.trim()); // Sử dụng trim() để loại bỏ khoảng trắng thừa
    }

    // Gửi request GET với các HttpParams đã xây dựng
    // Angular sẽ tự động nối các params này vào URL dưới dạng query string
    return this.http.get<PagedResult<ProductProperty>>(
      `${this.baseApiUrl}/product-properties`,
      { params: params, withCredentials: true }
    );
  }

  getAllUniquePropertyNames(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseApiUrl}/product-property-names`, { withCredentials: true });
  }

  deleteProductProperty(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/product-properties/${id}`;
    return this.http.delete<ProductProperty>(apiUrl, { withCredentials: true });
  }
  
  updateProductProperty(id: number, productProperties: ProductProperty) {
    let apiUrl: string = `${this.baseApiUrl}/product-properties/${id}`;
    return this.http.patch<ProductProperty>(apiUrl, productProperties, { withCredentials: true });
  }
}
