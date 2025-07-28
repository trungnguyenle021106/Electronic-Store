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
  + Gửi email  qua máy chủ SMTP (Quên mật khẩu).
+ Quản lý nội dung động:
  + Khả năng quản lý và hiển thị nội dung trang động thông qua các thực thể Filter linh hoạt.
     
## Kiến trúc
Dự án được xây dựng dựa trên Kiến trúc Microservice hiện đại để đảm bảo khả năng mở rộng, độc lập và dễ bảo trì. Mỗi service sử dụng Clean Architecture.
+ Ocelot API Gateway: Đóng vai trò là điểm vào duy nhất cho tất cả các yêu cầu từ Frontend, định tuyến chúng hiệu quả đến các Microservice phù hợp.
+ Các Microservice chính:
  + Product Service: Quản lý thông tin sản phẩm và các thuộc tính sản phẩm.
  + Order Service: Xử lý quy trình đặt hàng và trạng thái đơn hàng.
  + User Service: Quản lý tài khoản người dùng, xác thực (JWT) và phân quyền.
  + Content Management Service: Quản lý các thực thể Filter để kiểm soát nội dung động.
+ Mẫu thiết kế (Design Patterns): Hệ thống sử dụng rộng rãi các mẫu Unit of Work và Repository để tương tác với cơ sở dữ liệu.
  
## Công nghệ sử dụng
+ Backend (ASP.NET Core .NET 8):
  + ASP.NET Core .NET 8: Framework chính cho tất cả các Microservice.
  + JWT (JSON Web Tokens): Để xác thực người dùng an toàn.
  + SignalR: Kích hoạt giao tiếp thời gian thực cho các tính năng như cập nhật đơn hàng.
  + Entity Framework Core: ORM (Object-Relational Mapper) để tương tác với cơ sở dữ liệu.
  + Ocelot: API Gateway để định tuyến yêu cầu.
  + API Verifalia: Được sử dụng để xác minh địa chỉ email.
  + SMTP Client: Để gửi email giao dịch.
+ Frontend (Angular 19):
  + Angular 19
  + Angular Material
+ Cơ sở dữ liệu:
  + Microsoft SQL Server
+ Công cụ & Môi trường:
  + Docker: Được sử dụng để đóng gói và điều phối cả các dịch vụ API và ứng dụng frontend.
+ Visual Studio 2022 / Visual Studio Code
+ Swagger UI (để kiểm thử và tài liệu hóa API)
