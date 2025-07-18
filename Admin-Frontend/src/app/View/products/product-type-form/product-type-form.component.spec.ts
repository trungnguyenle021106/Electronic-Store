import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductTypeFormComponent } from './product-type-form.component';

describe('ProductTypeFormComponent', () => {
  let component: ProductTypeFormComponent;
  let fixture: ComponentFixture<ProductTypeFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ProductTypeFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductTypeFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
