import { Injectable } from '@angular/core';
import { Filter } from '../../Model/Filter/Filter';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { PagedResult } from '../../Model/Product/DTO/Response/PagedResult';
import { CreateUpdateFilterRequest } from '../../Model/Filter/DTO/CreateUpdateFilterRequest';
import { ProductProperty } from '../../Model/Product/ProductProperty';

@Injectable({
  providedIn: 'root'
})
export class ContentManagementService {
  private baseApiUrl = 'http://localhost:5293/gateway'; //gateway/content-managements

  constructor(private http: HttpClient) { }

  // Phương thức để gửi POST request
  createFilter(filter: Filter, productPropertyIDs: number[]): Observable<Filter> {
    const createFilterRequest: CreateUpdateFilterRequest = {
      Filter: filter,
      productPropertyIDs: productPropertyIDs
    }
    let apiUrl: string = `${this.baseApiUrl}/filters`;
    return this.http.post<Filter>(apiUrl, createFilterRequest, { withCredentials: true });
  }


  getPagedFilters(
    currentPage: number = 1,
    pageSize: number = 10,
    searchText: string = '',
    filter: string = ''
  ): Observable<PagedResult<Filter>> {

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
    return this.http.get<PagedResult<Filter>>(
      `${this.baseApiUrl}/filters`,
      { params: params, withCredentials: true }
    );
  }

  deleteFilter(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/filters/${id}`;
    return this.http.delete<Filter>(apiUrl, { withCredentials: true });
  }

  updateFilter(id: number, filter: Filter, productPropertyIDs: number[]) {
    let apiUrl: string = `${this.baseApiUrl}/filters/${id}`;
    const updateFilterRequest: CreateUpdateFilterRequest = {
      Filter: filter,
      productPropertyIDs: productPropertyIDs
    }
    return this.http.patch<Filter>(apiUrl, updateFilterRequest, { withCredentials: true });
  }

  getFilterByID(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/filters/${id}`;
    return this.http.get<Filter>(apiUrl, { withCredentials: true });
  }

  getAllPropertiesOfFilter(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/products/${id}/product-properties`;
    return this.http.get<ProductProperty[]>(apiUrl, { withCredentials: true });
  }
}
