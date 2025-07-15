import { Injectable } from '@angular/core';
import { Filter } from '../../Model/Filter/Filter';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateFilterRequest } from '../../Model/Product/DTO/Request/CreateFilterRequest';

@Injectable({
  providedIn: 'root'
})
export class ContentManagementService {
 private baseApiUrl = 'http://localhost:5293';
  constructor(private http: HttpClient) { }

  // Phương thức để gửi POST request
  createFilter(createFilterRequest: CreateFilterRequest): Observable<Filter> {
    let apiUrl: string = `${this.baseApiUrl}/filters`;
    return this.http.post<Filter>(apiUrl, createFilterRequest, { withCredentials: true });
  }

}
