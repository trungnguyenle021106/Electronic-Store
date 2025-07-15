import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateProductComponent } from './View/create-product/create-product.component';
import { LoginComponent } from './View/login/login.component';
import { authGuard } from './guards/auth.guard';
import { noAuthGuard } from './guards/no-auth.guard';
import { ContentManagementComponent } from './View/content-management/content-management.component';
import { ProductComponent } from './View/products/product/product.component';
import { PropertyComponent } from './View/products/property/property.component';
import { ProductTypeComponent } from './View/products/product-type/product-type.component';
import { ProductBrandComponent } from './View/products/product-brand/product-brand.component';
import { ProductFormComponent } from './View/products/product-form/product-form.component';

const routes: Routes = [
  // Route cho trang đăng nhập - KHÔNG CÓ GUARD
  { path: 'login', component: LoginComponent, canActivate: [noAuthGuard] },

  // Các Route còn lại - ĐỀU CÓ GUARD BẢO VỆ
  // Mẹo: Bạn có thể nhóm các route được bảo vệ vào một object route chung
  // để tránh lặp lại 'canActivate: [authGuard]' cho mỗi route.
  {
    path: '', // Route gốc, sẽ chuyển hướng tới ProductComponent sau khi xác thực
    component: ProductComponent,
    canActivate: [authGuard]
  },
  {
    path: 'product',
    component: ProductComponent,
    canActivate: [authGuard]
  },
  {
    path: 'product-form',
    component: ProductFormComponent,
    canActivate: [authGuard]
  },
  {
    path: 'product-type',
    component: ProductTypeComponent,
    canActivate: [authGuard]
  },
  {
    path: 'product-brand',
    component: ProductBrandComponent,
    canActivate: [authGuard]
  },
  {
    path: 'property',
    component: PropertyComponent,
    canActivate: [authGuard]
  },
  {
    path: 'create-product',
    component: CreateProductComponent,
    canActivate: [authGuard]
  },
  {
    path: 'content-management',
    component: ContentManagementComponent,
    canActivate: [authGuard]
  },
  // Route wildcard cho các đường dẫn không khớp, chuyển hướng về một trang nào đó
  // Ví dụ, chuyển hướng về 'product' sau khi xác thực
  { path: '**', redirectTo: '/product', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
