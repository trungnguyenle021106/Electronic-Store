import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductProperty } from '../../Model/Product/ProductProperty';
import { Filter } from '../../Model/Filter/Filter';
import { ContentManagementService } from '../../Service/ContentManagement/content-management.service';
import { ProductService } from '../../Service/Product/product.service';
import { Product } from '../../Model/Product/Product';

interface FilterData {
  position: string;
  productProperty: ProductProperty[];
}

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

  latestLaptops: Product[] = [];
  latestMonitors: Product[] = [];
  latestMice: Product[] = []; // Chuột (Mice)
  latestHeadphones: Product[] = [];
  latestKeyboards: Product[] = [];

  filterDatas: FilterData[] = [];

  productTypeNames = {
    laptop: 'Laptop',
    monitor: 'Màn hình',
    mouse: 'Chuột',
    headphone: 'Tai nghe',
    keyboard: 'Bàn phím'
  };

  constructor(private contentManagementService: ContentManagementService, private productService: ProductService) {

  }

  ngOnInit() {
    this.loadFilters();
    this.loadLatestProducts();


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
        this.InitLoadFilterData();
      }, error: (error) => {
        console.log(error);
      }
    })
  }

  GetProductPropertiesFilterData(position: string): ProductProperty[] {
    return this.filterDatas.find(fd => fd.position == position)?.productProperty ?? []
  }

  GetProductPropertyNamesFilterData(position: string): string[] {
    let ppName: string[] = []

    const ppOfFilter = this.filterDatas.find(ele => ele.position == position)?.productProperty ?? [];
    this.filterDatas.forEach(fd => {
      ppOfFilter.forEach(pp => {
        const isNameAlreadyPresent = ppName.find(name => name == pp.Name)
        if (!isNameAlreadyPresent) {
          ppName.push(pp.Name);
        }
      });
    });
    return ppName;
  }

  private InitLoadFilterData() {
    this.LoadFilterData('HomeMouse');
    this.LoadFilterData('HomeKeyBoard');
    this.LoadFilterData('HomeEar');
    this.LoadFilterData('HomeScreen');
    this.LoadFilterData('HomeLaptop');
  }

  private LoadFilterData(position: string) {
    const filter = this.filters.find(f => f.Position == position)

    if (filter && filter.ID) {
      this.contentManagementService.getAllPropertiesOfFilter(filter?.ID).subscribe({
        next: (response) => {
          let filterData: FilterData = { position: position, productProperty: [] }

          let ppOfFilter: ProductProperty[] = []
          ppOfFilter = response

          ppOfFilter.forEach(element => {
            filterData.productProperty.push(element);
          });

          this.filterDatas.push(filterData);
        }, error: (error) => {
          console.log(error);
        }
      });
    }
  }

  loadLatestProducts(): void {
    // Tải Laptop
    this.productService.getLatestProducts('Laptop').subscribe({
      next: (products) => {
        this.latestLaptops = products;
      },
      error: (err) => console.error('Lỗi khi tải Laptop mới nhất:', err)
    });

    // Tải Màn hình
    this.productService.getLatestProducts('Màn hình').subscribe({
      next: (products) => {
        this.latestMonitors = products;
      },
      error: (err) => console.error('Lỗi khi tải Màn hình mới nhất:', err)
    });

    // Tải Chuột
    this.productService.getLatestProducts('Chuột').subscribe({
      next: (products) => {
        this.latestMice = products;
      },
      error: (err) => console.error('Lỗi khi tải Chuột mới nhất:', err)
    });

    // Tải Tai nghe
    this.productService.getLatestProducts('Tai nghe').subscribe({
      next: (products) => {
        this.latestHeadphones = products;
      },
      error: (err) => console.error('Lỗi khi tải Tai nghe mới nhất:', err)
    });

    // Tải Bàn phím
    this.productService.getLatestProducts('Bàn phím').subscribe({
      next: (products) => {
        this.latestKeyboards = products;
      },
      error: (err) => console.error('Lỗi khi tải Bàn phím mới nhất:', err)
    });
  }

  GetProductPropertyID(Name: string, Description: string): number | undefined {
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