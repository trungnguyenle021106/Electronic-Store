import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HomeComponent } from './View/home/home.component';
import { HeaderComponent } from './View/header/header.component';
import { NavigationComponent } from './View/navigation/navigation.component';
import { FooterComponent } from './View/footer/footer.component';
import { CategoryComponent } from './View/category/category.component';
import { SearchComponent } from './View/search/search.component';
import { ProductComponent } from './View/product/product.component';
import { CartComponent } from './View/cart/cart.component';
import { AccountComponent } from './View/account/account.component';
import { OrderdetailComponent } from './View/orderdetail/orderdetail.component';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { AuthInterceptor } from './Interceptors/interceptor';
import { LoginComponent } from './View/form/login/login.component';
import { SignUpComponent } from './View/form/sign-up/sign-up.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AccountInformationComponent } from './View/form/account-information/account-information.component';
import { AccountOrderComponent } from './View/form/account-order/account-order.component';


@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    HeaderComponent,
    NavigationComponent,
    FooterComponent,
    CategoryComponent,
    SearchComponent,
    ProductComponent,
    CartComponent,
    AccountComponent,
    OrderdetailComponent,
    LoginComponent,
    SignUpComponent,
    AccountInformationComponent,
    AccountOrderComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ReactiveFormsModule,
    FormsModule
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
