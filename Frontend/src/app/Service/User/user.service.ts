import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Customer } from '../../Model/User/Customer';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private baseApiUrl = 'http://localhost:5293';
  constructor(private http: HttpClient) { }

  getMyProfile() {
    let apiUrl: string = `${this.baseApiUrl}/gateway/usersapi/customers/me`;
    return this.http.get<Customer>(apiUrl, { withCredentials: true });
  }
}
