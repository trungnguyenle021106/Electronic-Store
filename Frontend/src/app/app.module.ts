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
import { ProductDetailComponent } from './View/product-detail/product-detail.component';
import { ConfirmDialogComponent } from './View/dialogs/confirm-dialog/confirm-dialog.component';
import { ErrorDialogComponent } from './View/dialogs/error-dialog/error-dialog.component';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatListModule } from '@angular/material/list';
import { MatFormFieldModule } from '@angular/material/form-field';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ForgetPasswordComponent } from './View/form/forget-password/forget-password.component'; 

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
    AccountOrderComponent,
    ProductDetailComponent,
    ConfirmDialogComponent,
    ErrorDialogComponent,
    ForgetPasswordComponent,
    
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ReactiveFormsModule,
    FormsModule,
    MatButtonModule,
    BrowserAnimationsModule, 
    MatDialogModule, MatFormFieldModule, MatCheckboxModule, MatIconModule, MatSelectModule, MatListModule,
    MatSnackBarModule,
    MatTableModule,
     MatProgressSpinnerModule, 
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
