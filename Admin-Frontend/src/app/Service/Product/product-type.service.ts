import { Injectable } from '@angular/core';
import { ProductType } from '../../Model/Product/ProductType';
import { PagedResult } from '../../Model/Product/DTO/Response/PagedResult';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductTypeService {
 private baseApiUrl = 'http://localhost:5293/gateway/product-apis';

  constructor(private http: HttpClient) { }

  // Phương thức để gửi POST request
  createProductType(listCreate: ProductType[]): Observable<any> {
    let apiUrl: string = `${this.baseApiUrl}/product-types`;
    return this.http.post<any>(apiUrl, listCreate, { withCredentials: true });
  }


  getPagedProductTypes(
    currentPage: number = 1,
    pageSize: number = 10,
    searchText: string = '',
    filter: string = ''
  ): Observable<PagedResult<ProductType>> {

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
    return this.http.get<PagedResult<ProductType>>(
      `${this.baseApiUrl}/product-types`,
      { params: params, withCredentials: true }
    );
  }

  deleteProductType(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/product-types/${id}`;
    return this.http.delete<ProductType>(apiUrl, { withCredentials: true });
  }

  updateProductType(id: number, productBrand: ProductType) {
    let apiUrl: string = `${this.baseApiUrl}/product-types/${id}`;
    return this.http.patch<ProductType>(apiUrl, productBrand, { withCredentials: true });
  }
}
