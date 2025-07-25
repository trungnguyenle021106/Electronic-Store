import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CustomerInformation } from '../../Model/User/CustomerInformation';
import { Customer } from '../../Model/User/Customer';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private baseApiUrl = 'http://localhost:5293/gateway/user-apis';
  constructor(private http: HttpClient) { }

  getMyProfile() {
    let apiUrl: string = `${this.baseApiUrl}/customers/me`;
    return this.http.get<CustomerInformation>(apiUrl, { withCredentials: true });
  }

  UpdateCustomerInformation(customerID: number, newCustomer: Customer) {
    let apiUrl: string = `${this.baseApiUrl}/customers/${customerID}`;
    return this.http.put<CustomerInformation>(apiUrl, newCustomer, { withCredentials: true });
  }
}
