import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductPropertiesSelectorComponent } from './product-properties-selector.component';

describe('ProductPropertiesSelectorComponent', () => {
  let component: ProductPropertiesSelectorComponent;
  let fixture: ComponentFixture<ProductPropertiesSelectorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ProductPropertiesSelectorComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductPropertiesSelectorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
