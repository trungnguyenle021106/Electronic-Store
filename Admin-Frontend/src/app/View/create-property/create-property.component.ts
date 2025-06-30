import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductProperty } from '../../Model/Product/ProductProperty';
import { PropertyService } from '../../Service/Property/property.service';


@Component({
  selector: 'app-create-property',
  standalone: false,
  templateUrl: './create-property.component.html',
  styleUrl: './create-property.component.css'
})
export class CreatePropertyComponent {
  @Output() statusEmitter = new EventEmitter<boolean>();
  propertyForm: FormGroup;

  constructor(private fb: FormBuilder, private productProperty: PropertyService) {
    this.propertyForm = this.fb.group({
      name: [
        '',
        [
          Validators.required,
          Validators.pattern(/^[a-zA-Z\s]+$/) // Chỉ cho phép chữ cái và khoảng trắng
        ]
      ],
      description: [
        '',
        [
          Validators.required,
          Validators.pattern(/^[a-zA-Z0-9\s]+$/) // Chỉ cho phép chữ cái, số và khoảng trắng
        ]
      ]
    });
  }

  get name() {
    return this.propertyForm.get('name');
  }

  get description() {
    return this.propertyForm.get('description');
  }

  onSubmit() {
    
    this.propertyForm.markAllAsTouched();
    if (this.propertyForm.valid) {
      const newProductProperty: ProductProperty = this.propertyForm.value;
      this.productProperty.createProductProperty(newProductProperty).subscribe({
        next: (response) => {
          console.log(response);
        },
        error: (error) => {
          console.error('Error fetching products:', error);
        },
      });
    } else {
      console.log('Form is invalid');
    }
  }

  CloseCreateProperty(): void {
    this.statusEmitter.emit(false);
  }
}
