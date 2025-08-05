import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CustomerInformation } from '../../Model/User/CustomerInformation';
import { Account } from '../../Model/User/Account';
import { PagedResult } from '../../Model/PagedResult';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private baseApiUrl = 'http://localhost:5293/gateway/user-apis';
  constructor(private http: HttpClient) { }

  getCustomerInformationByCustomerID(customerID: number) {
    let apiUrl: string = `${this.baseApiUrl}/customers/${customerID}/customer-information`;
    return this.http.get<CustomerInformation>(apiUrl, { withCredentials: true });
  }

  getCustomerInformationByAccountID(accountID: number) {
    let apiUrl: string = `${this.baseApiUrl}/accounts/${accountID}/customer-information`;
    return this.http.get<CustomerInformation>(apiUrl, { withCredentials: true });
  }

  updateAccount(
    id: number,
    newStatus: string,) {
    let apiUrl: string = `${this.baseApiUrl}/accounts/${id}/status`;
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    const requestBody = JSON.stringify(newStatus);
    return this.http.patch<Account>(apiUrl, requestBody, { headers: headers, withCredentials: true });
  }

  getPagedAccounts(
    currentPage: number = 1,
    pageSize: number = 10,
    searchText: string = '',
  ): Observable<PagedResult<Account>> {
    let params = new HttpParams()
      .set('page', currentPage.toString())
      .set('pageSize', pageSize.toString());

    if (searchText && searchText.trim().length > 0) {
      params = params.set('searchText', searchText.trim());
    }


    let apiUrl: string = `${this.baseApiUrl}/accounts`;
    return this.http.get<PagedResult<Account>>(apiUrl, { params: params, withCredentials: true });
  }

}
