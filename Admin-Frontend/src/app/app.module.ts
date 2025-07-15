
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { NavigationComponent } from './View/navigation/navigation.component';
import { CreateProductComponent } from './View/create-product/create-product.component';

import { FormsModule, ReactiveFormsModule } from '@angular/forms'; // <-- Import ReactiveFormsModule
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { LoginComponent } from './View/login/login.component';
import { AuthInterceptor } from './Interceptors/interceptor';
import { NgModule } from '@angular/core';
import { ContentManagementComponent } from './View/content-management/content-management.component';

import { ScrollingModule } from '@angular/cdk/scrolling';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialogModule } from '@angular/material/dialog';
import { PropertyComponent } from './View/products/property/property.component';
import { ProductComponent } from './View/products/product/product.component';
import { PropertyFormComponent } from './View/products/property-form/property-form.component';
import { ProductPropertiesSelectorComponent } from './View/products/product-properties-selector/product-properties-selector.component';
import { ProductTypeComponent } from './View/products/product-type/product-type.component';
import { ProductBrandComponent } from './View/products/product-brand/product-brand.component';
import { ProductBrandFormComponent } from './View/products/product-brand-form/product-brand-form.component';
import { ProductTypeFormComponent } from './View/products/product-type-form/product-type-form.component';
import { ProductFormComponent } from './View/products/product-form/product-form.component';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatListModule } from '@angular/material/list';
@NgModule({
  declarations: [
    AppComponent,
    ProductComponent,
    NavigationComponent,
    CreateProductComponent,
    PropertyComponent,
    LoginComponent,
    ContentManagementComponent,
    ProductPropertiesSelectorComponent,
    PropertyFormComponent,
    ProductTypeComponent,
    ProductBrandComponent,
    ProductBrandFormComponent,
    ProductTypeFormComponent,
    ProductFormComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ReactiveFormsModule,
    FormsModule,
    ScrollingModule,
    BrowserAnimationsModule, // Bắt buộc cho Angular Material
    MatTableModule,          // Bảng MatTable
    MatPaginatorModule,      // Phân trang MatPaginator
    MatSortModule,           // Sắp xếp MatSort
    MatButtonModule,          // Các nút bấm
    MatDialogModule, MatFormFieldModule,MatCheckboxModule, MatIconModule, MatSelectModule, MatListModule,
  ],
  providers: [
    provideHttpClient(),
    provideHttpClient(withInterceptorsFromDi()),
    // Đăng ký Interceptor
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
