import { Injectable } from '@angular/core';

import { Order } from '../../Model/Order/Order';
import { PagedResult } from '../../Model/PagedResult';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Product } from '../../Model/Product/Product';
import { OrderDetail } from '../../Model/Order/OrderDetail';
import { OrderItem } from '../../Model/Order/OrderItem';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private baseApiUrl = 'http://localhost:5293/gateway/order-apis';
  constructor(private http: HttpClient) {
  }

  getPagedOrders(
    currentPage: number = 1,
    pageSize: number = 10,
    searchText: string = '',
    isIncrease?: boolean
  ): Observable<PagedResult<Order>> {
    let params = new HttpParams()
      .set('page', currentPage.toString())
      .set('pageSize', pageSize.toString());

    if (searchText && searchText.trim().length > 0) {
      params = params.set('searchText', searchText.trim());
    }

    if (isIncrease !== undefined && isIncrease !== null) { // Kiểm tra để đảm bảo isIncrease có giá trị
      params = params.set('isIncrease', isIncrease.toString()); // Chuyển boolean sang chuỗi
    }

    let apiUrl: string = `${this.baseApiUrl}/orders`;
    return this.http.get<PagedResult<Order>>(apiUrl, { params: params, withCredentials: true });
  }

  updateOrder(
    id: number,
    newStatus: string,) {
    let apiUrl: string = `${this.baseApiUrl}/orders/${id}/status`;
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    const requestBody = JSON.stringify(newStatus);
    return this.http.patch<Order>(apiUrl, requestBody,{ headers: headers, withCredentials: true });
  }

  getOrderDetailsOfOrder(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/orders/${id}/order-details`;
    return this.http.get<OrderDetail[]>(apiUrl, { withCredentials: true });
  }

  getProductsOfOrder(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/orders/${id}/products`;
    return this.http.get<Product[]>(apiUrl, { withCredentials: true });
  }

  getOrderItemsOfOrder(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/orders/${id}/order-items`;
    return this.http.get<OrderItem[]>(apiUrl, { withCredentials: true });
  }

  getOrderByID(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/orders/${id}`;
    return this.http.get<Order>(apiUrl, { withCredentials: true });
  }
}
