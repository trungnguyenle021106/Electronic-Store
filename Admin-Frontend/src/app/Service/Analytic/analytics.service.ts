import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AnalyzeOrderRequest } from '../../Model/Analytic/AnalyzeOrderRequest';
import { ProductStatistics } from '../../Model/Analytic/ProductStatistics';
import { ProductStatisticResponse } from '../../Model/Analytic/ProductStatisticResponse';
import { OrderByDate } from '../../Model/Analytic/OrderByDate';

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {

  private apiUrl = 'http://localhost:5293/gateway';
  constructor(private http: HttpClient) { }

  analyzeOrderByDate(body: AnalyzeOrderRequest): Observable<any> {
    const url = `${this.apiUrl}/analytics-apis/order-by-date`;
    return this.http.post(url, body, { withCredentials: true });
  }


  analyzeProductStatistics(body: ProductStatistics[]): Observable<any> {
    const url = `${this.apiUrl}/analytics-apis/product-statistics`;
    return this.http.post(url, body, { withCredentials: true });
  }


private formatDate(date: Date): string {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = (d.getMonth() + 1).toString().padStart(2, '0');
    const day = d.getDate().toString().padStart(2, '0');
    return `${year}-${month}-${day}`;
}

// Hàm gửi API đã được sửa
getOrderByDateAnalytics(startDate: Date, endDate: Date): Observable<OrderByDate[]> {
  const url = `${this.apiUrl}/analytics-apis/order-by-date`;
  
  const params = new HttpParams()
    .set('a', this.formatDate(startDate))
    .set('b', this.formatDate(endDate));

  return this.http.get<OrderByDate[]>(url, { params, withCredentials: true });
}


  getProductStatisticsAnalytics(top: number): Observable<ProductStatisticResponse[]> {
    const url = `${this.apiUrl}/analytics-apis/product-statistics`;
    const params = new HttpParams()
      .set('top', top.toString());

    return this.http.get<ProductStatisticResponse[]>(url, { params, withCredentials: true });
  }
}
