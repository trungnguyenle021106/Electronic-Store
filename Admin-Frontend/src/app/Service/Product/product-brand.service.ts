import { Injectable } from '@angular/core';
import { ProductBrand } from '../../Model/Product/ProductBrand';
import { Observable } from 'rxjs';
import { PagedResult } from '../../Model/Product/DTO/Response/PagedResult';
import { HttpClient, HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ProductBrandService {
  private baseApiUrl = 'http://localhost:5293/gateway/product-apis';

  constructor(private http: HttpClient) { }

  // Phương thức để gửi POST request
  createProductBrand(listCreate: ProductBrand[]): Observable<any> {
    let apiUrl: string = `${this.baseApiUrl}/product-brands`;
    return this.http.post<any>(apiUrl, listCreate, { withCredentials: true });
  }


  getPagedProductBrands(
    currentPage: number = 1,
    pageSize: number = 10,
    searchText: string = '',
    filter: string = ''
  ): Observable<PagedResult<ProductBrand>> {

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
    return this.http.get<PagedResult<ProductBrand>>(
      `${this.baseApiUrl}/product-brands`,
      { params: params, withCredentials: true }
    );
  }

  deleteProductBrand(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/product-brands/${id}`;
    return this.http.delete<ProductBrand>(apiUrl, { withCredentials: true });
  }

  updateProductBrand(id: number, productBrand: ProductBrand) {
    let apiUrl: string = `${this.baseApiUrl}/product-brands/${id}`;
    return this.http.patch<ProductBrand>(apiUrl, productBrand, { withCredentials: true });
  }
}
