import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CustomerInformation } from '../../Model/User/CustomerInformation';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private baseApiUrl = 'http://localhost:5293';
  constructor(private http: HttpClient) { }

  getMyProfile() {
    let apiUrl: string = `${this.baseApiUrl}/gateway/usersapi/customers/me`;
    return this.http.get<CustomerInformation>(apiUrl, { withCredentials: true });
  }
}
