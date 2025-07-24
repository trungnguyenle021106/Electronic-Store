export interface PagedResult<T > {
    Items: T[];        // Thay vì 'data', giờ là 'Items'
    TotalCount: number; // Thay vì 'totalRecords', giờ là 'TotalCount'
    Page: number;      // Thay vì 'pageIndex', giờ là 'Page'
    PageSize: number;  // Giữ nguyên 'pageSize'
    TotalPages: number; // Thuộc tính mới
    HasPreviousPage: boolean; // Thuộc tính mới
    HasNextPage: boolean;     // Thuộc tính mới
}