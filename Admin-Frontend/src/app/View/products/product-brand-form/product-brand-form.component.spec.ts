import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductBrandFormComponent } from './product-brand-form.component';

describe('ProductBrandFormComponent', () => {
  let component: ProductBrandFormComponent;
  let fixture: ComponentFixture<ProductBrandFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ProductBrandFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductBrandFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
