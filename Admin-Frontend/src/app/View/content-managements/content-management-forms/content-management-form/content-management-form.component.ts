import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ProductProperty } from '../../../../Model/Product/ProductProperty';
import { ActivatedRoute, Router } from '@angular/router';
import { ContentManagementService } from '../../../../Service/ContentManagement/content-management.service';
import { Subject, takeUntil } from 'rxjs';
import { Filter } from '../../../../Model/Filter/Filter';
import { ConfirmDialogComponent } from '../../../dialogs/confirm-dialog/confirm-dialog.component';
import { ErrorDialogComponent } from '../../../dialogs/error-dialog/error-dialog.component';

@Component({
  selector: 'app-content-management-form',
  standalone: false,
  templateUrl: './content-management-form.component.html',
  styleUrl: './content-management-form.component.css'
})
export class ContentManagementFormComponent {
  readonly dialog = inject(MatDialog);

  filterForm: FormGroup;
  curFilterID: number | undefined
  typeFilterForm: string = "";
  selectedProperties: ProductProperty[] = [];
  senderSelectedItem: ProductProperty | undefined;
  senderUnSelectedItem: ProductProperty | undefined;
  actionName: string = "";
  describeAction: string = "";


  private destroyComponent$ = new Subject<void>();
  constructor(private contentManagementService: ContentManagementService, private fb: FormBuilder,
    private route: ActivatedRoute, private router: Router
  ) {
    this.filterForm = this.fb.group({
      Position: ['', Validators.required],
    });
  }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.typeFilterForm = params['typeFilterForm'];

      // Log giá trị sau khi nó đã được gán
      console.log("Giá trị của typeFilterForm sau khi nhận từ URL:", this.typeFilterForm);

      if (this.typeFilterForm == "Create") {
        this.setForCreate();
      }
      else if (this.typeFilterForm == "Update") {
        const filterID: number = params['itemID'];
        this.curFilterID = filterID;
        this.setForUpdate(filterID);
      }
    });
  }

  ngAfterViewInit() {

  }


  onSubmit(): void {
    this.filterForm.markAllAsTouched();
    if (this.selectedProperties.length == 0 && this.typeFilterForm == "Create") {
      this.openErrorDialog("300ms", "150ms", "Lỗi thêm filter", "Filter phải chứa ít nhất 1 thuộc tính")
    }
    else if (this.selectedProperties.length == 0 && this.typeFilterForm == "Update") {
      this.openErrorDialog("300ms", "150ms", "Lỗi cập nhật filter", "Filter phải chứa ít nhất 1 thuộc tính")
    }
    else if (this.filterForm.valid && this.typeFilterForm == "Create") {
      this.openConfirmDialog("300ms", "150ms", "thêm filter");
    } else if (this.filterForm.valid && this.typeFilterForm == "Update") {
      this.openConfirmDialog("300ms", "150ms", "cập nhật filter");
    }
  }

  handleConfirmAction() {

    if (this.typeFilterForm == "Create") {
      this.CreateFilter();
    } else if (this.typeFilterForm == "Update") {
      this.UpdateProduct();
    }
  }

  CreateFilter(): void {
    const newFilter: Filter = {
      ID: 0,
      Position: this.filterForm.value.Position,
    }
    const productPropertyIDs: number[] = this.selectedProperties.map(prop => prop.ID);
    this.contentManagementService.createFilter(newFilter, productPropertyIDs).subscribe({
      next: (response) => {
        this.router.navigate(['content-management'])
      }, error: (error) => {
        this.openErrorDialog("300ms", "150ms", "Lỗi thêm filter", error.message)
      }
    });

  }

  UpdateProduct(): void {
    const newFilter: Filter = {
      ID: this.curFilterID ?? 0,  
      Position: this.filterForm.value.Position,
    }

    const productPropertyIDs: number[] = this.selectedProperties.map(prop => prop.ID);
    if (newFilter.ID) {
      this.contentManagementService.updateFilter(newFilter.ID, newFilter, productPropertyIDs).subscribe({
        next: (response) => {
          this.router.navigate(['content-management'])
        }, error: (error) => {
          this.openErrorDialog("300ms", "150ms", "Lỗi cập nhật filter", error.message)
          console.log(error)
        }
      });
    }
  }


  private setForCreate() {
    this.actionName = "Thêm filter"
    this.describeAction = "Thêm filter"
  }

  private setForUpdate(filterID: number) {
    this.actionName = "Cập nhật"
    this.describeAction = "Cập nhật filter"
    this.contentManagementService.getFilterByID(filterID).subscribe({
      next: (response) => {
        console.log(response)
        this.filterForm.patchValue(response);
      }, error: (error) => {
        console.log(error)
      }
    });
    this.loadselectProductProperties();
  }


  private loadselectProductProperties(): void {
    if (this.curFilterID) {
      this.contentManagementService.getAllPropertiesOfFilter(this.curFilterID)
        .pipe(takeUntil(this.destroyComponent$)) // Hủy đăng ký khi component bị hủy
        .subscribe(
          {
            next: (response) => {
              this.selectedProperties = response;
            },
            error: (error) => {
              console.error('Error loading product properties:', error);
            }
          }
        );
    }
  }

  openConfirmDialog(enterAnimationDuration: string, exitAnimationDuration: string, actionName: string): void {
    this.dialog.open(ConfirmDialogComponent, {
      width: '300px', // Tăng width để dễ nhìn hơn
      enterAnimationDuration,
      exitAnimationDuration,
      // ✨ TRUYỀN PHƯƠNG THỨC VÀO ĐÂY QUA THUỘC TÍNH 'data' ✨
      data: {
        actionName: actionName,
        onConfirm: () => this.handleConfirmAction()
      }
    });
  }

  openErrorDialog(enterAnimationDuration: string, exitAnimationDuration: string, errorTitle: string, errorMessage: string): void {
    this.dialog.open(ErrorDialogComponent, {
      width: '300px', // Kích thước phù hợp với dialog lỗi
      enterAnimationDuration,
      exitAnimationDuration,
      // Truyền tiêu đề và thông báo lỗi vào dialog
      data: {
        title: errorTitle,
        message: errorMessage
      },
      disableClose: true, // Thường là lỗi thì không cho click ra ngoài đóng
      hasBackdrop: true, // Luôn có backdrop
    });
  }

  OnItemSelected(productProperty: ProductProperty) {
    const item: ProductProperty = { ID: productProperty.ID, Name: productProperty.Name, Description: productProperty.Description };
    this.senderSelectedItem = item;
    this.selectedProperties.push(productProperty);
  }

  OnItemUnSelected(productProperty: ProductProperty) {
    const item: ProductProperty = { ID: productProperty.ID, Name: productProperty.Name, Description: productProperty.Description };
    this.senderUnSelectedItem = item;
    this.selectedProperties = this.selectedProperties.filter(item => item.ID !== productProperty.ID);
  }


  ngOnDestroy(): void {
    this.destroyComponent$.next();
    this.destroyComponent$.complete();
  }
}
