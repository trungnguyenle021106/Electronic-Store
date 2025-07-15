import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private baseApiUrl = 'http://localhost:5293';
  constructor(private http: HttpClient) { }

  // getCustomerByIDForCustomer()
  // {
  //   let apiUrl: string = `${this.baseApiUrl}/gateway/usersapi/customers/4`;
  //   return this.http.get<LoginResponse>(apiUrl, {withCredentials : true});
  // }
  
}
