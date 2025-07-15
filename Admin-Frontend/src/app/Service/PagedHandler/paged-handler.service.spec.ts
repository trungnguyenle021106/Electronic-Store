import { TestBed } from '@angular/core/testing';

import { PagedHandlerService } from './paged-handler.service';

describe('PagedHandlerService', () => {
  let service: PagedHandlerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PagedHandlerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
