
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ProductComponent } from './View/product/product.component';
import { NavigationComponent } from './View/navigation/navigation.component';
import { CreateProductComponent } from './View/create-product/create-product.component';
import { PropertyComponent } from './View/property/property.component';
import { CreatePropertyComponent } from './View/create-property/create-property.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms'; // <-- Import ReactiveFormsModule
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { LoginComponent } from './View/login/login.component';
import { AuthInterceptor } from './Interceptors/interceptor';
import { NgModule } from '@angular/core';


@NgModule({
  declarations: [
    AppComponent,
    ProductComponent,
    NavigationComponent,
    CreateProductComponent,
    PropertyComponent,
    CreatePropertyComponent,
    LoginComponent
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
