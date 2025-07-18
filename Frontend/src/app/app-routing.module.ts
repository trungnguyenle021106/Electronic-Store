import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './View/home/home.component';
import { CategoryComponent } from './View/category/category.component';
import { SearchComponent } from './View/search/search.component';
import { ProductComponent } from './View/product/product.component';
import { CartComponent } from './View/cart/cart.component';
import { AccountComponent } from './View/account/account.component';
import { OrderdetailComponent } from './View/orderdetail/orderdetail.component';
import { authGuard } from './guards/auth.guard';

const routes: Routes =
  [
    { path: '', component: HomeComponent },
    { path: 'category', component: CategoryComponent },
    { path: 'search', component: SearchComponent },
    { path: 'product', component: ProductComponent, title: 'product' },
    { path: 'cart', component: CartComponent, title: 'cart' },
    {
      path: 'account',
      component: AccountComponent,
      title: 'account',
      canActivate: [authGuard], // <-- THÊM DÒNG NÀY ĐỂ BẢO VỆ ROUTE TÀI KHOẢN
      children: [ // <-- THÊM MẢNG CHILDREN VÀO ĐÂY
        {
          path: '', // Route rỗng sẽ khớp với '/account'
          redirectTo: 'information', // Tùy chọn: chuyển hướng '/account' đến '/account/information'
          pathMatch: 'full'
        },
        {
          path: 'order', // Route rỗng sẽ khớp với '/account'
          component: AccountComponent, // Tùy chọn: chuyển hướng '/account' đến '/account/information'
          title: 'account-order'
        },
        {
          path: 'information', // Khớp với '/account/information'
          component: AccountComponent, // <-- Vẫn hiển thị AccountComponent
          title: 'account-information'
        },
      ]
    },
    { path: 'orderdetail/:id', component: OrderdetailComponent, title: 'orderdetail' },
  ];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
