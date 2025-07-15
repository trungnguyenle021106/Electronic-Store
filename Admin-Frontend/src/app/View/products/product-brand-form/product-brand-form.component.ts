import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ProductBrand } from '../../../Model/Product/ProductBrand';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductBrandService } from '../../../Service/Product/product-brand.service';

@Component({
  selector: 'app-product-brand-form',
  standalone: false,
  templateUrl: './product-brand-form.component.html',
  styleUrl: './product-brand-form.component.css'
})
export class ProductBrandFormComponent {
  @Output() statusEmitter = new EventEmitter<boolean>();
  @Input() typePropertyForm: string = '';
  @Input() curProductBrand: ProductBrand | undefined;

  actionName: string = "";
  describeAction: string = "";

  propertyForm: FormGroup;

  isConFirmDisplayed: boolean = false;

  constructor(private fb: FormBuilder, private productBrand: ProductBrandService) {
    this.propertyForm = this.fb.group({
      name: [
        '',
        [
          Validators.required
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
    const newProductBrand: ProductBrand = this.propertyForm.value;
    let listCreate: ProductBrand[] = [];
    listCreate.push(newProductBrand);
    this.productBrand.createProductBrand(listCreate).subscribe({
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
    this.describeAction = "Tạo mới hãng sản phẩm"
  }

  private Update() {
    if (this.curProductBrand) {
       const newProductBrand: ProductBrand = this.propertyForm.value;
      this.productBrand.updateProductBrand(this.curProductBrand.ID, newProductBrand).subscribe({
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
    this.describeAction = "Cập nhật hãng sản phẩm"

    if (this.curProductBrand?.Name) {
      this.propertyForm.get('name')?.setValue(this.curProductBrand.Name);
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
