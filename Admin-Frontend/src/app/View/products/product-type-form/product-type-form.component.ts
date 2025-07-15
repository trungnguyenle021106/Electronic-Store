import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ProductType } from '../../../Model/Product/ProductType';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductTypeService } from '../../../Service/Product/product-type.service';

@Component({
  selector: 'app-product-type-form',
  standalone: false,
  templateUrl: './product-type-form.component.html',
  styleUrl: './product-type-form.component.css'
})
export class ProductTypeFormComponent {

  @Output() statusEmitter = new EventEmitter<boolean>();
  @Input() typePropertyForm: string = '';
  @Input() curProductType: ProductType | undefined;

  actionName: string = "";
  describeAction: string = "";

  propertyForm: FormGroup;

  isConFirmDisplayed: boolean = false;

  constructor(private fb: FormBuilder, private productType: ProductTypeService) {
    this.propertyForm = this.fb.group({
      name: [
        '',
        [
          Validators.required,
          Validators.pattern(/^[\p{L}\s]+$/u)// Chỉ cho phép chữ cái và khoảng trắng
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

  onSubmit() {
    this.propertyForm.markAllAsTouched();
    if (this.propertyForm.valid) {
      this.DisplayConfirmForm(true);
    } else {
      console.log('Form is invalid');
    }
  }

  private Create() {
    const newProductType: ProductType = this.propertyForm.value;
    let listCreate: ProductType[] = [];
    listCreate.push(newProductType);
    this.productType.createProductType(listCreate).subscribe({
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
    this.describeAction = "Tạo mới loại sản phẩm"
  }

  private Update() {
    if (this.curProductType) {
       const newProductType: ProductType = this.propertyForm.value;
      this.productType.updateProductType(this.curProductType.ID, newProductType).subscribe({
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
    this.describeAction = "Cập nhật loại sản phẩm"

    if (this.curProductType?.Name) {
      this.propertyForm.get('name')?.setValue(this.curProductType.Name);
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
