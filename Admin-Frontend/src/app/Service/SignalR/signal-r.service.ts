import { Injectable } from '@angular/core';
import { filter, map, Observable, Subject, takeUntil } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { ProductProperty } from '../../Model/Product/ProductProperty';

export interface ProductPropertyPayload {
  producProperty: ProductProperty; // Bạn có thể định nghĩa ProductProperty interface nếu muốn
  message: string;
}

interface HubEventPayload {
  hubUrl: string;    // URL của Hub gửi sự kiện
  eventName: string; // Tên của sự kiện (ví dụ: "ProductAdded")
  data: any[];       // Các tham số của sự kiện
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private connections: Map<string, signalR.HubConnection> = new Map();
  // Subject để phát ra tất cả các sự kiện nhận được từ bất kỳ Hub nào
  private allHubEvents = new Subject<HubEventPayload>();
  private destroy$ = new Subject<void>();

  constructor() { }

  /**
   * Khởi tạo hoặc lấy một HubConnection cho URL cụ thể.
   * Nếu đã có kết nối, trả về kết nối hiện có.
   */
  private async getOrCreateHubConnection(hubUrl: string): Promise<signalR.HubConnection> {
    if (this.connections.has(hubUrl)) {
      const existingConnection = this.connections.get(hubUrl)!;
      if (existingConnection.state === signalR.HubConnectionState.Connected) {
        console.log(`[SignalR Service] Already connected to ${hubUrl}`);
        return existingConnection;
      }
      // Nếu đã có nhưng chưa kết nối, thử kết nối lại
      if (existingConnection.state === signalR.HubConnectionState.Disconnected) {
        try {
          await existingConnection.start();
          console.log(`[SignalR Service] Reconnected to ${hubUrl}`);
          return existingConnection;
        } catch (err) {
          console.error(`Error reconnecting to ${hubUrl}:`, err);
          this.connections.delete(hubUrl); // Xóa để tạo lại nếu lỗi
        }
      }
    }

    // Tạo kết nối mới nếu chưa có hoặc lỗi
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        withCredentials: true
      })
      .withAutomaticReconnect()
      .build();

    newConnection.onreconnected(() => console.log(`[SignalR Service] Reconnected to ${hubUrl}`));
    newConnection.onclose(err => console.log(`[SignalR Service] Connection to ${hubUrl} closed:`, err));

    try {
      await newConnection.start();
      console.log(`[SignalR Service] Connection started for ${hubUrl}`);
      this.connections.set(hubUrl, newConnection);
      return newConnection;
    } catch (err) {
      console.error(`Error starting connection for ${hubUrl}:`, err);
      throw err;
    }
  }

  /**
   * Đăng ký lắng nghe một sự kiện cụ thể từ một Hub cụ thể.
   * @param hubUrl URL của Hub (ví dụ: '/productNotificationHub')
   * @param eventName Tên của sự kiện (ví dụ: 'ProductAdded')
   * @returns Observable phát ra dữ liệu khi sự kiện xảy ra.
   * Lưu ý: Dữ liệu được trả về là một mảng (any[]) chứa các tham số sự kiện từ BE.
   * Component cần tự xử lý (cast/destructure) mảng này.
   */
  public on<T>(hubUrl: string, eventName: string): Observable<T> {
    // Đảm bảo kết nối được thiết lập
    this.getOrCreateHubConnection(hubUrl).then(connection => {
      // Đăng ký listener. SignalR sẽ xử lý việc đăng ký nhiều lần cho cùng một sự kiện.
      // Dòng code kiểm tra `connection.listeners.some(...)` đã được loại bỏ.
      connection.on(eventName, (...args: any[]) => {
        console.log(`[SignalR Service] Received event '${eventName}' from '${hubUrl}' with data:`, args);
        this.allHubEvents.next({ hubUrl, eventName, data: args });
      });
    }).catch(err => {
      console.error(`Failed to establish connection for ${hubUrl} to listen for ${eventName}:`, err);
    });

    // Trả về một Observable được lọc chỉ cho sự kiện và Hub mong muốn
    return this.allHubEvents.asObservable().pipe(
      filter(payload => payload.hubUrl === hubUrl && payload.eventName === eventName),
      takeUntil(this.destroy$), // Hủy đăng ký khi service bị hủy
      // Map payload thành kiểu T mong muốn
      // Ở đây, chúng ta trả về `payload.data` và Component sẽ chịu trách nhiệm ép kiểu hoặc destructure
      map(payload => {
        // Tùy thuộc vào cách bạn gửi dữ liệu từ backend, bạn có thể cần điều chỉnh ở đây.
        // Ví dụ:
        // - Nếu bạn gửi 1 đối tượng duy nhất từ BE: await Clients.All.SendAsync("Event", myObject);
        //   => payload.data sẽ là [myObject], bạn cần return payload.data[0] as T;
        // - Nếu bạn gửi nhiều tham số từ BE: await Clients.All.SendAsync("Event", param1, param2);
        //   => payload.data sẽ là [param1, param2], bạn cần return { param1: payload.data[0], param2: payload.data[1] } as T;
        // Để tổng quát, chúng ta sẽ trả về payload.data, và component sẽ xử lý.
        return payload.data as T; // Ép kiểu trực tiếp. TypeScript sẽ không phàn nàn ở đây.
      })
    );
  }

  /**
   * Ngắt kết nối khỏi tất cả các Hub đã được thiết lập.
   */
  public async stopAllConnections(): Promise<void> {
    for (const [hubUrl, connection] of this.connections.entries()) {
      if (connection.state !== signalR.HubConnectionState.Disconnected) {
        try {
          await connection.stop();
          console.log(`[SignalR Service] Connection to ${hubUrl} stopped.`);
        } catch (err) {
          console.error(`Error stopping connection for ${hubUrl}:`, err);
        }
      }
    }
    this.connections.clear();
  }

  ngOnDestroy(): void {
    this.stopAllConnections();
    this.destroy$.next();
    this.destroy$.complete();
    this.allHubEvents.complete();
  }
}
