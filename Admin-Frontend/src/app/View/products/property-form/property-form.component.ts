import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PropertyService } from '../../../Service/Product/property.service';
import { ProductProperty } from '../../../Model/Product/ProductProperty';

@Component({
  selector: 'app-property-form',
  standalone: false,
  templateUrl: './property-form.component.html',
  styleUrl: './property-form.component.css'
})
export class PropertyFormComponent {
  @Output() statusEmitter = new EventEmitter<boolean>();
  @Input() typePropertyForm: string = '';
  @Input() curProductProperty: ProductProperty | undefined;

  actionName: string = "";
  describeAction: string = "";

  propertyForm: FormGroup;

  isConFirmDisplayed: boolean = false;

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
          Validators.pattern(/^[\p{L}\s]+$/u) // Chỉ cho phép chữ cái, số và khoảng trắng
        ]
      ]
    });
  }

  ngOnInit() {
    if (this.typePropertyForm == "Create") {
      this.SetForCreateAction();
    } else if (this.typePropertyForm == "Update") {
      this.SetForUpdateAction();
    }
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
      this.DisplayConfirmForm(true);
    } else {
      console.log('Form is invalid');
    }
  }

  private Create() {
    const newProductProperty: ProductProperty = this.propertyForm.value;
    let listCreate: ProductProperty[] = [];
    listCreate.push(newProductProperty);
    this.productProperty.createProductProperty(listCreate).subscribe({
      next: (response) => {
        this.ClosePropertyForm();
      },
      error: (error) => {
        alert(error.message);
        console.log(error);
      },
    });

  }

  SetForCreateAction() {
    this.actionName = "Tạo mới"
    this.describeAction = "Tạo mới thuộc tính sản phẩm"
  }

  private Update() {
    if (this.curProductProperty) {
      const newProductProperty: ProductProperty = this.propertyForm.value;
      this.productProperty.updateProductProperty(this.curProductProperty.ID, newProductProperty).subscribe({
        next: (response) => {
          this.ClosePropertyForm();
        },
        error: (error) => {
          alert(error.message);
          console.log(error);
        },
      });
    }
  }

  SetForUpdateAction() {
    this.actionName = "Cập nhật"
    this.describeAction = "Cập nhật thuộc tính sản phẩm"

    if (this.curProductProperty?.Name) {
      this.propertyForm.get('name')?.setValue(this.curProductProperty.Name);
    }
    if (this.curProductProperty?.Name) {
      this.propertyForm.get('description')?.setValue(this.curProductProperty.Description);
    }
  }

  DisplayConfirmForm(isShow: boolean): void {
    this.isConFirmDisplayed = isShow;
  }

  OnAccept(): void {
    if (this.typePropertyForm == "Create") {
      this.Create();
    } else if (this.typePropertyForm == "Update") {
      this.Update();
    }
  }

  ClosePropertyForm(): void {
    this.statusEmitter.emit(false);
  }
}
