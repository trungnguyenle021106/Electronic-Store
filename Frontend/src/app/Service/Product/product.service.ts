import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResult } from '../../Model/PagedResult';
import { Product } from '../../Model/Product/Product';
import { ProductProperty } from '../../Model/Product/ProductProperty';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private baseApiUrl = 'http://localhost:5293/gateway/product-apis';
  constructor(private http: HttpClient) { }

  getLatestProducts(productTypeName : string)
  {
     let params = new HttpParams()
      .set('productTypeName', productTypeName)

    let apiUrl: string = `${this.baseApiUrl}/products/latest`;
    return this.http.get<Product[]>(apiUrl, { params: params, withCredentials: true });
  }

  getPagedProducts(
    currentPage: number = 1,
    pageSize: number = 10,
    searchText: string | null = null, // Có thể là null ban đầu
    productTypeName: string | null = null,
    productBrandName: string | null = null,
    productStatus: string | null = null,
    productPropertyIds: string | null = null,
    isIncrease: boolean = true,
  ): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('page', currentPage.toString())
      .set('pageSize', pageSize.toString())
      .set('isIncrease', isIncrease);

    if (searchText && searchText.trim().length > 0) {
      params = params.set('searchText', searchText.trim());
    }

    if (productPropertyIds && productPropertyIds.trim().length > 0) {
      params = params.set('ProductPropertyIds', productPropertyIds.trim());
    }

    if (productTypeName && productTypeName.trim().length > 0) {
      params = params.set('ProductTypeName', productTypeName.trim());
    }

    if (productBrandName && productBrandName.trim().length > 0) {
      params = params.set('ProductBrandName', productBrandName.trim());
    }

    if (productStatus && productStatus.trim().length > 0) {
      params = params.set('ProductStatus', productStatus.trim());
    }
    let apiUrl: string = `${this.baseApiUrl}/products`;
    return this.http.get<PagedResult<Product>>(apiUrl, { params: params, withCredentials: true });
  }

  getProductByID(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/products/${id}`;
    return this.http.get<Product>(apiUrl, { withCredentials: true });
  }

  getAllPropertiesOfProduct(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/products/${id}/product-properties`;
    return this.http.get<ProductProperty[]>(apiUrl, { withCredentials: true });
  }
}
