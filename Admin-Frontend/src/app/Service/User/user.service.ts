import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CustomerInformation } from '../../Model/User/CustomerInformation';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private baseApiUrl = 'http://localhost:5293/gateway/user-apis';
  constructor(private http: HttpClient) { }

  getCustomerInformationByCustomerID(customerID : number)
  {
    let apiUrl: string = `${this.baseApiUrl}/customers/${customerID}/customer-information`;
    return this.http.get<CustomerInformation>(apiUrl, {withCredentials : true});
  }
  
}
