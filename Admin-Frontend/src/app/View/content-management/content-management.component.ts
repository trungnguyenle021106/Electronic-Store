import { Component } from '@angular/core';
import { ContentManagementService } from '../../Service/ContentManagement/content-management.service';
import { CreateFilterRequest } from '../../Model/Product/DTO/Request/CreateFilterRequest';


@Component({
  selector: 'app-content-management',
  standalone: false,
  templateUrl: './content-management.component.html',
  styleUrl: './content-management.component.css'
})
export class ContentManagementComponent {
  constructor(private contenService: ContentManagementService) {

  }

  ngOnInit() {
    const data: CreateFilterRequest = {
      Filter: {
        Position: 'string44',
      },
      productPropertyIDs: [1,2], // Start with an empty array of numbers
    };
    this.contenService.createFilter(data).subscribe({
      next: (response) => {

      },
      error: (error) => {
        console.error('Lỗi:', error);
      }
    });

  }
}
