import { Injectable } from '@angular/core';
import { Order } from '../../Model/Order/Order';
import { OrderItem } from '../../Model/Order/OrderItem';
import { OrderDetail } from '../../Model/Order/OrderDetailt';
import { HttpClient, HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private baseApiUrl = 'http://localhost:5293/gateway/order-apis';
  constructor(private http: HttpClient) {
  }


  createOrder(orderDetails: OrderDetail[]) {
    let apiUrl: string = `${this.baseApiUrl}/orders`;
    return this.http.post<Order>(apiUrl, orderDetails, { withCredentials: true });
  }

  getOrderItemsOfOrder(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/orders/${id}/order-items`;
    return this.http.get<OrderItem[]>(apiUrl, { withCredentials: true });
  }

  getOrderCurrentCustomer(status?: string) {
    let apiUrl: string = `${this.baseApiUrl}/orders/me`;
    let params = new HttpParams();
    if (status) { // Kiểm tra để đảm bảo status có giá trị
      params = params.set('status', status);
    }

    return this.http.get<Order[]>(apiUrl, {
      params: params, // Truyền đối tượng HttpParams vào đây
      withCredentials: true
    });
  }
}
