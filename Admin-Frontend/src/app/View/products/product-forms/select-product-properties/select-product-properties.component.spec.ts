import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectProductPropertiesComponent } from './select-product-properties.component';

describe('SelectProductPropertiesComponent', () => {
  let component: SelectProductPropertiesComponent;
  let fixture: ComponentFixture<SelectProductPropertiesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SelectProductPropertiesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SelectProductPropertiesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
