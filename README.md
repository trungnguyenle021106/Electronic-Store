# Cửa hàng bán đồ điện tử
Web được thiết kế để bán các sản phẩm điện tử như laptop, bàn phím, tai nghe, chuột và nhiều sản phẩm khác, có thể mở rộng bán nhiều loại sản phẩm nhờ cấu trúc thực thể thuộc tính sản phẩm (product property). Web có quản lý cho sản phẩm, đơn hàng, tài khoản người dùng và quản lý nội dung trang linh hoạt thông qua các thực thể Filter có thể cấu hình, giúp dễ dàng mở rộng sang các loại sản phẩm đa dạng.

## Tính năng
Nền tảng này cung cấp bộ tính năng phong phú để quản lý và vận hành một cửa hàng điện tử trực tuyến:
+ Quản lý sản phẩm toàn diện:
  + Các thao tác CRUD (Tạo, Đọc, Cập nhật, Xóa) đầy đủ cho các sản phẩm điện tử.
  + Thuộc tính sản phẩm linh hoạt để dễ dàng mở rộng và quản lý các danh mục sản phẩm đa dạng.
+ Quản lý đơn hàng:
  + Tạo, xem và cập nhật trạng thái đơn hàng.
  + Cập nhật trạng thái đơn hàng theo thời gian thực sử dụng SignalR để cập nhật đơn hàng mới ngay lập tức.
+ Quản lý tài khoản người dùng:
  + Chức năng đăng ký và đăng nhập an toàn sử dụng JWT (JSON Web Tokens) để xác thực.
  + Xác thực và kiểm tra email bằng API Verifalia.
  + Gửi email qua máy chủ SMTP (Quên mật khẩu).
+ Quản lý nội dung động:
  + Khả năng quản lý và hiển thị nội dung trang động thông qua các thực thể Filter linh hoạt.
+ Thống kê :
  + Thống kê doanh thu, số lượng đơn hàng, tỷ lệ hủy đơn hàng trong khoảng thời gian
  + Thống kê các sản phẩm bán chạy 
## Kiến trúc
Dự án được xây dựng dựa trên Kiến trúc Microservice hiện đại để đảm bảo khả năng mở rộng, độc lập và dễ bảo trì. Mỗi service sử dụng Clean Architecture.
+ Ocelot API Gateway: Đóng vai trò là điểm vào duy nhất cho tất cả các yêu cầu từ Frontend, định tuyến chúng hiệu quả đến các Microservice phù hợp.
+ Các Microservice chính:
  + Product Service: Quản lý thông tin sản phẩm và các thuộc tính sản phẩm. Chứa và xử lý liên quan đến các thực thể Product, ProductType, ProductBrand, ProductProperty, ProductPropertyDetail
  + Order Service: Xử lý quy trình đặt hàng và trạng thái đơn hàng. Chứa và xử lý liên quan đến các thực thể Order, OrderDetail
  + User Service: Quản lý tài khoản người dùng, xác thực (JWT). Chứa và xử lý liên quan đến các thực thể Account, Customer, RefreshToken
  + Content Management Service: Quản lý các thực thể Filter để kiểm soát nội dung động. Chứa và xử lý liên quan đến các thực thể Filter, FilterDetail
  + Analytic Serrvice : Thống kê doanh thu, số lượng đơn hàng, tỷ lệ hủy đơn hàng, sản phẩm bán chạy . Chứa và xử lý liên quan đến các thực thể OrderByDate, ProductStatistics
+ Mẫu thiết kế (Design Patterns): Hệ thống sử dụng rộng rãi các mẫu Unit of Work và Repository để tương tác với cơ sở dữ liệu. 
+ Rest API
  
## Công nghệ sử dụng
+ Backend (ASP.NET Core .NET 8):
  + Rest API
  + ASP.NET Core .NET 8: Framework chính cho tất cả các Microservice.
  + JWT (JSON Web Tokens): Để xác thực người dùng an toàn.
  + SignalR: Kích hoạt giao tiếp thời gian thực cho các tính năng như cập nhật đơn hàng.
  + Entity Framework Core: ORM (Object-Relational Mapper) để tương tác với cơ sở dữ liệu.
  + Ocelot: API Gateway để định tuyến yêu cầu.
  + API Verifalia: Được sử dụng để xác minh địa chỉ email.
  + SMTP Client: Để gửi email giao dịch.
  + AWS S3 : Sử dụng để lưu hình ảnh sản phẩm
+ Frontend (Angular 19):
  + Angular 19
  + Angular Material
+ Cơ sở dữ liệu:
  + Microsoft SQL Server
+ Công cụ & Môi trường:
  + Docker: Được sử dụng để đóng gói và điều phối các dịch vụ API và Frontend.
+ Visual Studio 2022 / Visual Studio Code
+ Swagger UI (để kiểm thử và tài liệu hóa API)

## Các bước cài đặt
1. Clone Repository
2. Cấu hình:
  + Tạo các cơ sở dữ liệu  ProductDb, OrderDb, UserDb, ContentDb trên phiên bản SQL Server cho từng dịch vụ bằng cách thủ công hoặc sử dụng Migration, cấu hình chuỗi kết nối đặt ở file env
  + Cấu hình cho JWT trong file env về issuer, signingkey, audience (Thêm AccessTokenExpirationMinutes trong user service) 
  + Đăng ký tài khoản Verifalia, lấy khóa API và cấu hình tại file appsetting.json
  + Đăng ký AWS S3 và tạo bucket, sử dụng Shared AWS credentials file và cấu hình tại file docker compose
  + Cập nhật chi tiết máy chủ SMTP của bạn (host, port, username, password) trong appsettings.json.
3. Build : Chạy lệnh docker compose up --build hoặc docker compose up --build -d tại thư mục gốc dự án

## Một số hình ảnh web
### TRANG ADMIN
<img width="1904" height="858" alt="image" src="https://github.com/user-attachments/assets/639380e1-34f5-40c0-8cae-2af191b42dfd" />
<img width="1651" height="927" alt="image" src="https://github.com/user-attachments/assets/6c5bba94-483e-4356-9fd0-d878f330f38f" />
<img width="1689" height="759" alt="image" src="https://github.com/user-attachments/assets/87461789-5073-4e14-8ef9-ac73ee15a668" />
<img width="1653" height="462" alt="image" src="https://github.com/user-attachments/assets/6db3118b-b417-454b-b7a8-d314cae5a1f8" />
### TRANG CHÍNH
<img width="1918" height="721" alt="image" src="https://github.com/user-attachments/assets/3ff4a2ec-56fe-4646-8a34-de9b1057e747" />
<img width="1909" height="636" alt="image" src="https://github.com/user-attachments/assets/e7d0a8d9-fac5-4281-9cc4-152534a21a2d" />
<img width="1523" height="926" alt="image" src="https://github.com/user-attachments/assets/be8c0150-128c-4a8a-8efc-ba7561b238c8" />
<img width="1561" height="946" alt="image" src="https://github.com/user-attachments/assets/edc43767-909a-4f72-9a08-7ed5b2c7f801" />
<img width="1407" height="949" alt="image" src="https://github.com/user-attachments/assets/772da138-e170-43e9-8d98-cf6cde2dc4f4" />
<img width="1245" height="453" alt="image" src="https://github.com/user-attachments/assets/cec9af69-13bd-4551-9780-efe237611c97" />
<img width="1287" height="761" alt="image" src="https://github.com/user-attachments/assets/2149f131-cf4b-4f57-8818-6e4da49eab8e" />
<img width="1277" height="637" alt="image" src="https://github.com/user-attachments/assets/e6c09428-8f15-4d32-90dd-81967bd2f7d3" />
<img width="1256" height="586" alt="image" src="https://github.com/user-attachments/assets/1b4e14ac-ccc2-485d-9173-b34c75ed177b" />
<img width="1004" height="908" alt="image" src="https://github.com/user-attachments/assets/a9c19796-e4a7-49c1-b8dc-2227b4707985" />
<img width="573" height="764" alt="image" src="https://github.com/user-attachments/assets/21ace258-376d-49c8-9609-104fc9d3be3a" />




