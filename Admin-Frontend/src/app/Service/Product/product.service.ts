import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ProductDTO } from '../../Model/Product/DTO/Response/ProductDTO';
import { PagedResult } from '../../Model/Product/DTO/Response/PagedResult';
import { ProductProperty } from '../../Model/Product/ProductProperty';
import { Product } from '../../Model/Product/Product';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private baseApiUrl = 'http://localhost:5293/gateway/product-apis';
  constructor(private http: HttpClient) { }

  createProduct(
    productData: Product,          // Dữ liệu sản phẩm (ví dụ: { name: 'Laptop', price: 1200 })
    productPropertyIDs: number[],  // Mảng các ID thuộc tính (ví dụ: [1, 5, 8])
    productFile: File              // Đối tượng File (từ input type="file")
  ): Observable<any> { // Thay vì <Product>, có thể trả về <any> hoặc interface của response API của bạn
    let apiUrl: string = `${this.baseApiUrl}/products`;

    // Khởi tạo FormData
    const formData = new FormData();
    formData.append('Name', productData.Name);
    formData.append('Quantity', productData.Quantity.toString());
    formData.append('ProductBrandID', productData.ProductBrandID.toString());
    formData.append('Description', productData.Description);
    formData.append('Price', productData.Price.toString());
    formData.append('Status', productData.Status);
    formData.append('ProductTypeID', productData.ProductTypeID.toString());

    productPropertyIDs.forEach(id => {
      formData.append('ProductPropertyIDs', id.toString()); // Convert number to string
    });

    formData.append('File', productFile, productFile.name);

    // Gửi FormData. HttpClient sẽ tự động thiết lập Content-Type: multipart/form-data.
    return this.http.post<any>(apiUrl, formData, { withCredentials: true });
  }

  getPagedProducts(
    currentPage: number = 1,
    pageSize: number = 10,
    searchText: string = '',
    filter: string = ''
  ): Observable<PagedResult<ProductDTO>> {
    let params = new HttpParams()
      .set('page', currentPage.toString()) // Chuyển số sang chuỗi
      .set('pageSize', pageSize.toString()); // Chuyển số sang chuỗi

    // Thêm searchText vào params nếu nó có giá trị (không rỗng, không khoảng trắng)
    if (searchText && searchText.trim().length > 0) {
      params = params.set('searchText', searchText.trim()); // Sử dụng trim() để loại bỏ khoảng trắng thừa
    }

    // Thêm filter vào params nếu nó có giá trị (không rỗng, không khoảng trắng)
    if (filter && filter.trim().length > 0) {
      params = params.set('filter', filter.trim()); // Sử dụng trim() để loại bỏ khoảng trắng thừa
    }
    let apiUrl: string = `${this.baseApiUrl}/products`;
    return this.http.get<PagedResult<ProductDTO>>(apiUrl, { params: params, withCredentials: true });
  }

  getAllPropertiesOfProduct(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/products/${id}/product-properties`;
    return this.http.get<ProductProperty[]>(apiUrl, { withCredentials: true });
  }

  deleteProduct(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/products/${id}`;
    return this.http.delete<Product>(apiUrl, { withCredentials: true });
  }

  updateProduct(
    id: number,
    productData: Product,
    productPropertyIDs: number[],
    productFile: File | null) {
    let apiUrl: string = `${this.baseApiUrl}/products/${id}`;

    const formData = new FormData();
    formData.append('Name', productData.Name);
    formData.append('Quantity', productData.Quantity.toString());
    formData.append('ProductBrandID', productData.ProductBrandID.toString());
    formData.append('Description', productData.Description);
    formData.append('Price', productData.Price.toString());
    formData.append('Status', productData.Status);
    formData.append('ProductTypeID', productData.ProductTypeID.toString());
    if (productData.Image) {
      formData.append('Image', productData.Image.toString());
    }

    productPropertyIDs.forEach(id => {
      formData.append('ProductPropertyIDs', id.toString()); // Convert number to string
    });
    if (productFile) {
      formData.append('File', productFile, productFile.name);
    }
    return this.http.patch<Product>(apiUrl, formData, { withCredentials: true });
  }

  getProductByID(id: number) {
    let apiUrl: string = `${this.baseApiUrl}/products/${id}`;
    return this.http.get<Product>(apiUrl, { withCredentials: true });
  }
}
