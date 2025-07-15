import { Injectable } from '@angular/core';
import { PagedResult } from '../../Model/Product/DTO/Response/PagedResult';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

export interface page<T> {
  Items: T[]; // Bạn có thể định nghĩa ProductProperty interface nếu muốn
  nunmberPage: number;
}

@Injectable({
  providedIn: 'root'
})
export class PagedHandlerService<T extends { ID: number }> {
  private startID: number = 0; // bằng với đợt lấy id phần tử đầu tiên trong mảng ProductDTO
  private lastID: number = 0;// bằng với đợt lấy id phần tử cuối trong mảng ProductDTO
  private totalPageMax: number = 1;
  private highestPageNumber: number = 1;
  private pageSize: number = 10;
  private maxItem: number = 18

  private isLoading: boolean = false;

  private request: Observable<PagedResult<T>>;
  private CurrentItems = new BehaviorSubject<T[]>([]);
  readonly currentItems$ = this.CurrentItems.asObservable();

  private baseApiUrl = 'http://localhost:5293/gateway';
  private subUrl = "";
  constructor(private http: HttpClient) {
    this.request = this.http.get<PagedResult<T>>(this.baseApiUrl, { withCredentials: true });
  }

  getDataFirstTime(CurrentItems: T[], url: string, tableHeight: number, offsetHeight: number, columnHeight: number) {
    this.pageSize = Math.floor((tableHeight - offsetHeight) / columnHeight);
    console.log(this.pageSize)
    this.maxItem = this.pageSize * 2;
    this.subUrl = url;
    this.setRequest(1, this.pageSize);
    this.loadMoreData(CurrentItems, false);
  }

  private setRequest(page: number, pageSize: number) {
    this.request = this.http.get<PagedResult<T>>(`${this.baseApiUrl}/${this.subUrl}?page=${page}&pageSize=${pageSize}`, { withCredentials: true });
  }

  isHighestPageChange(CurrentItems: T[], item: T): boolean {
    if (CurrentItems.length == this.maxItem && this.highestPageNumber == this.totalPageMax) {
      this.totalPageMax++;
      return true;
    }

    if (CurrentItems.length < this.maxItem && this.highestPageNumber == this.totalPageMax) {
      const updatedItems = [...CurrentItems, item];
      console.log(updatedItems); // In ra mảng mới
      this.CurrentItems.next(updatedItems); // Truyền mảng mới
    }

    return false;
  }

  handleElementOutBound(visibleItemIDs: Set<number>, CurrentItems: T[]): void {

    const copiedItems: T[] = [...CurrentItems]; // Tạo bản sao để không ảnh hưởng đến tham chiếu gốc
    if (this.isLoading) {
      return;
    }

    let isScrollTop: boolean = true;

    const startIdVisible = visibleItemIDs.has(this.startID);
    const lastIdVisible = visibleItemIDs.has(this.lastID);
    console.log(
      'startID:', this.startID,
      'startIdVisible:', startIdVisible,
      'lastID:', this.lastID,
      'lastIdVisible:', lastIdVisible,
      'highestPageNumber:', this.highestPageNumber

    );
    if (lastIdVisible) {
      isScrollTop = false;
      console.log("DEBUG: Condition met: 'cuối mảng' (lastID is visible)");
    } else if (startIdVisible) { // Cẩn thận với thứ tự if/else if
      isScrollTop = true;
      console.log("DEBUG: Condition met: 'đầu mảng' (startID is visible)");
    } else {
      return; // Không làm gì nếu không thấy cả hai
    }
    // console.log('Giá trị của this.maxItem:', this.maxItem);
    // console.log('Giá trị của this.pageSize:', this.pageSize);

    const totalPageMaxCurItemHold: number = Math.ceil(this.maxItem / this.pageSize);

    console.log('Kết quả cuối cùng của totalPageMaxCurItemHold (sau Math.ceil):', totalPageMaxCurItemHold);
    // console.log("high :" + this.highestPageNumber)

    if (isScrollTop && this.highestPageNumber > totalPageMaxCurItemHold) {
      console.log("doTop");
      const prevPageNumber = this.highestPageNumber - totalPageMaxCurItemHold;
      // console.log("prevPageNumber :" + prevPageNumber)
      this.isLoading = true;
      this.setRequest(prevPageNumber, this.pageSize);
      this.loadMoreData(copiedItems, true);
    }
    else if (!isScrollTop && this.totalPageMax > this.highestPageNumber) {
      console.log("doBot");
      this.isLoading = true;
      this.setRequest(this.highestPageNumber + 1, this.pageSize);
      this.loadMoreData(copiedItems, false);
    }
  }

  private loadMoreData(CurrentItems: T[], isScollTop: boolean) {

    // this.request không có take(1) hay finalize ở đây theo yêu cầu của bạn
    this.request.subscribe({
      next: (response) => {
        this.totalPageMax = response.TotalPages;
        const processedData = this.HandleDataResponse(CurrentItems, response.Items, isScollTop);
        this.CurrentItems.next(processedData);
        this.isLoading = false; // <<< GIỮ DÒNG NÀY THEO CODE CỦA BẠN (nhưng finalize vẫn tốt hơn)
      },
      error: (error) => {
        console.error("loadMoreData: SUBSCRIBE ERROR triggered:", error);
        this.isLoading = false;
        console.groupEnd(); // Kết thúc nhóm log
      }
    });
  }

  private HandleDataResponse(CurrentItems: T[], ResponseItems: T[], isScollTop: boolean): T[] {

    if (CurrentItems.length == 0) {
      this.setStartLastID(ResponseItems);
      return ResponseItems;
    }

    if (isScollTop) {
      const startPos: number = CurrentItems.length - 1;
      CurrentItems.unshift(...ResponseItems);
      if (CurrentItems.length > this.maxItem) {
        const remainder = CurrentItems.length - this.pageSize;
        CurrentItems.splice(startPos, remainder);
      }
      this.highestPageNumber--; // <<< LƯU Ý: Đây là nơi highestPageNumber giảm khi cuộn lên

    } else {
      CurrentItems.push(...ResponseItems)
      if (CurrentItems.length > this.maxItem) {
        const remainder = CurrentItems.length - this.maxItem;
        CurrentItems.splice(0, remainder);
      }
      this.highestPageNumber++; // <<< LƯU Ý: Đây là nơi highestPageNumber tăng khi cuộn xuống
    }

    this.setStartLastID(CurrentItems);
    return CurrentItems;
  }

  private setStartLastID(CurrentItems: T[]): void {
    if (CurrentItems && CurrentItems.length > 0) { // Đảm bảo mảng không rỗng
      this.startID = CurrentItems[0].ID;
      this.lastID = CurrentItems[CurrentItems.length - 1].ID;
    }
    console.log(this.startID + ", " + this.lastID)
  }
}
