import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { CategoryComponent } from './category/category.component';
import { SearchComponent } from './search/search.component';
import { ProductComponent } from './product/product.component';
import { CartComponent } from './cart/cart.component';
import { AccountComponent } from './account/account.component';
import { OrderdetailComponent } from './orderdetail/orderdetail.component';

const routes: Routes =
  [
    { path: '', component: HomeComponent },
    { path: 'category', component: CategoryComponent },
    { path: 'search', component: SearchComponent },
    { path: 'product', component: ProductComponent, title: 'product' },
    { path: 'cart', component: CartComponent, title: 'cart' },
     { path: 'account', component: AccountComponent, title: 'account' },
      { path: 'orderdetail/:id', component: OrderdetailComponent, title: 'orderdetail' },
  ];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
