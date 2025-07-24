import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductProperty } from '../../Model/Product/ProductProperty';
import { Filter } from '../../Model/Filter/Filter';
import { ContentManagementService } from '../../Service/ContentManagement/content-management.service';
import { ProductService } from '../../Service/Product/product.service';
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  standalone: false,
})
export class HomeComponent {
  showSubMenu = false;
  ProductTypeName: string = '';

  productPropertyIDS: string = "";
  productPropertyNamesOfFilter: string[] = [];
  productPropertiesOfFilter: ProductProperty[] = [];
  filters: Filter[] = [];


  constructor(private contentManagementService: ContentManagementService, private productService: ProductService) {

  }

  ngOnInit() {
    this.loadFilters();
  }

  toggleSubMenu(status: boolean, ProductType: string): void {
    this.showSubMenu = status;
    this.ProductTypeName = ProductType;
  }

  private loadProductPropertiesOfFilter(id: number) {
    this.contentManagementService.getAllPropertiesOfFilter(id).subscribe({
      next: (response) => {
        this.productPropertiesOfFilter = response

        this.productPropertiesOfFilter.forEach(element => {
          const isNameAlreadyPresent = this.productPropertyNamesOfFilter.some(
            existingName => existingName === element.Name
          );

          if (!isNameAlreadyPresent) {
            this.productPropertyNamesOfFilter.push(element.Name);
          }
        });
      }, error: (error) => {
        console.log(error);
      }
    });
  }

  loadProductPropertyNamesOfFilter(position: string) {
    this.productPropertyNamesOfFilter = [];
    this.filters.forEach(element => {
      if (element.Position == position) {
        this.loadProductPropertiesOfFilter(element.ID ?? 0)
      }
    });
  }

  loadPropDes(propName: string): string[] {
    let list: string[] = [];
    this.productPropertiesOfFilter.forEach(element => {
      if (element.Name == propName) {
        list.push(element.Description);
      }
    });
    return list;
  }

  private loadFilters() {
    this.contentManagementService.getPagedFilters().subscribe({
      next: (response) => {
        this.filters = response.Items
      }, error: (error) => {

      }
    })
  }

  GetProductPropertyID(Name: string, Description: string) : number | undefined{
    const foundProp = this.productPropertiesOfFilter.find(p => p.Name == Name && p.Description == Description)
    if (foundProp) {
      return foundProp.ID; 
    }
    return foundProp;
  }

  GetFilterID(position: string): number | undefined { // Cập nhật kiểu trả về
    const foundFilter = this.filters.find(f => f.Position === position);
    if (foundFilter) {
      return foundFilter.ID; // Giả sử mỗi filter có thuộc tính ID
    }
    return undefined;
  }
}