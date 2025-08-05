import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { PagedResult } from '../../Model/PagedResult';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ProductBrand } from '../../Model/ProductBrand';

@Injectable({
  providedIn: 'root'
})
export class ProductBrandService {
  private baseApiUrl = 'http://localhost:5293/gateway/product-apis';

  constructor(private http: HttpClient) { }


  getPagedProductBrands(
    currentPage?: number | null,
    pageSize?: number | null,
    searchText: string = '',
    filter: string = ''
  ): Observable<PagedResult<ProductBrand>> {
    let params = new HttpParams()
    // Khởi tạo HttpParams
    if (currentPage && pageSize) {
      params
        .set('page', currentPage.toString()) // Chuyển số sang chuỗi
        .set('pageSize', pageSize.toString()); // Chuyển số sang chuỗi
    }
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
    return this.http.get<PagedResult<ProductBrand>>(
      `${this.baseApiUrl}/product-brands`,
      { params: params, withCredentials: true }
    );
  }


}
