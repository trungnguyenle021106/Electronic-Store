using CommonDto.ResultDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.OpenApi.Models;
using OrderService.Application.Service;
using OrderService.Application.Usecases;
using OrderService.Domain.DTO.Response;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.DTO;
using OrderService.Infrastructure.Socket;
namespace OrderService.Interface_Adapters.API
{
    public static class OrderAPI
    {
        public static void MapOrderEndpoints(this WebApplication app)
        {
            app.MapHub<OrderHub>("/orderHub")
           .RequireAuthorization("OnlyAdmin");

            MapCreateOrderUseCaseAPIs(app);
            MapGetOrderUsecaseAPIs(app);
            MapUpdateOrderUseCaseAPIs(app);
        }
        #region Create Order USECASE
        public static void MapCreateOrderUseCaseAPIs(this WebApplication app)
        {
            MapCreateOrder(app);
        }

        public static void MapCreateOrder(this WebApplication app)
        {
            app.MapPost("/orders", async (CreateOrderUC createOrderUC, [FromBody] List<OrderDetail> orderDetails,
                HttpContext httpContext, HandleResultApi handleResultApi, IHubContext<OrderHub> OrderHubContext) =>
            {
                ServiceResult<Order> result = await createOrderUC.
                CreateOrder(TokenService.GetJWTClaim(httpContext)?.CustomerID ?? 0, orderDetails);

                if (result.IsSuccess)
                {
                    await OrderHubContext.Clients.All.SendAsync(
                        "OrderChanged",
                        result.Item,
                        "OrderAdded"
                        );
                }
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyCustomer")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Tạo đơn hàng mới";
                operation.Description = "Tạo một đơn hàng mới với các chi tiết đơn hàng được cung cấp.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Orders" } };
                return operation;
            });
        }
        #endregion

        #region Get Order USECASE
        public static void MapGetOrderUsecaseAPIs(this WebApplication app)
        {
            MapGetOrdersByCustomerID(app);
            MapGetOrderByID(app);
            MapGetOrdetailOfOrder(app);
            GetPagedOrders(app);
            GetProductOfOrder(app);
            GetOrderItemOfOrder(app);
            MapGetOrdersCurrentCustomer(app);
        }

        public static void GetOrderItemOfOrder(this WebApplication app)
        {
            app.MapGet("/orders/{orderID}/order-items", async (GetOrderUC getOrderUC, int orderID, HandleResultApi handleResultApi, HttpContext httpContext) =>
            {
                ServiceResult<Order> resultSearch = await getOrderUC.GetOrderByID(orderID);
                if (!resultSearch.IsSuccess)
                {
                    return handleResultApi.MapServiceResultToHttp(resultSearch);
                }
                if ((resultSearch.Item.CustomerID == TokenService.GetJWTClaim(httpContext)?.CustomerID) || TokenService.GetJWTClaim(httpContext).Role == "Admin")
                {
                    ServiceResult<OrderItem> result = await getOrderUC.GetOrderItemOfOrder(resultSearch.Item.ID);
                    return handleResultApi.MapServiceResultToHttp(result);
                }
                return Results.Forbid();
            })
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy các Order Items của một Order";
                operation.Description = "Lấy danh sách các mục sản phẩm (Order Items) trong một đơn hàng cụ thể.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Orders" } };
                operation.Parameters[0].Description = "ID của đơn hàng.";
                return operation;
            });
        }

        public static void GetProductOfOrder(this WebApplication app)
        {
            app.MapGet("/orders/{orderID}/products", async (GetOrderUC getOrderUC, int orderID, HandleResultApi handleResultApi, HttpContext httpContext) =>
            {
                ServiceResult<Order> resultSearch = await getOrderUC.GetOrderByID(orderID);
                if (!resultSearch.IsSuccess)
                {
                    return handleResultApi.MapServiceResultToHttp(resultSearch);
                }
                if ((resultSearch.Item.CustomerID == TokenService.GetJWTClaim(httpContext)?.CustomerID) || TokenService.GetJWTClaim(httpContext).Role == "Admin")
                {
                    ServiceResult<Product> result = await getOrderUC.GetProductOfOrder(resultSearch.Item.ID);
                    return handleResultApi.MapServiceResultToHttp(result);
                }
                return Results.Forbid();
            })
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách sản phẩm trong một Order";
                operation.Description = "Lấy danh sách các sản phẩm thuộc một đơn hàng cụ thể.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Orders" } };
                operation.Parameters[0].Description = "ID của đơn hàng.";
                return operation;
            });
        }

        public static void GetPagedOrders(this WebApplication app)
        {
            app.MapGet("/orders", async (GetOrderUC getOrderUC, [FromQuery] string? searchText, [FromQuery] bool? isIncrease,
                [FromQuery] int page, [FromQuery] int pageSize, HandleResultApi handleResultApi) =>
            {
                ServiceResult<PagedResult<Order>> result = await getOrderUC.GetPagedOrder(page, pageSize, searchText, isIncrease);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách đơn hàng được phân trang";
                operation.Description = "Lấy danh sách đơn hàng được phân trang, có thể tìm kiếm và sắp xếp.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Orders" } };
                operation.Parameters[0].Description = "Văn bản tìm kiếm đơn hàng (nếu có).";
                operation.Parameters[1].Description = "Sắp xếp tăng dần (true) hay giảm dần (false).";
                operation.Parameters[2].Description = "Số trang cần lấy.";
                operation.Parameters[3].Description = "Số lượng đơn hàng trên mỗi trang.";
                return operation;
            });
        }

        public static void MapGetOrdersByCustomerID(this WebApplication app)
        {
            app.MapGet("/customers/{customerID}/orders", async (GetOrderUC getOrderUC, int customerID, HandleResultApi handleResultApi,
                [FromQuery] string? status) =>
            {
                ServiceResult<Order> result = await getOrderUC.GetOrdersByCustomerID(customerID, status);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách đơn hàng theo Customer ID";
                operation.Description = "Lấy danh sách đơn hàng của một khách hàng cụ thể. Yêu cầu quyền Admin.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Orders" } };
                operation.Parameters[0].Description = "ID của khách hàng.";
                operation.Parameters[1].Description = "Trạng thái đơn hàng cần lọc (nếu có).";
                return operation;
            });
        }

        public static void MapGetOrdersCurrentCustomer(this WebApplication app)
        {
            app.MapGet("/orders/me", async (GetOrderUC getOrderUC, HandleResultApi handleResultApi, HttpContext httpContext,
                [FromQuery] string? status) =>
            {
                int customerID = TokenService.GetJWTClaim(httpContext)?.CustomerID ?? 0;
                ServiceResult<Order> result = await getOrderUC.GetOrdersByCustomerID(customerID, status);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyCustomer")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách đơn hàng của khách hàng hiện tại";
                operation.Description = "Lấy danh sách đơn hàng của khách hàng đang đăng nhập.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Orders" } };
                operation.Parameters[0].Description = "Trạng thái đơn hàng cần lọc (nếu có).";
                return operation;
            });
        }

        public static void MapGetOrderByID(this WebApplication app)
        {
            app.MapGet("/orders/{orderId}", async (GetOrderUC getOrderUC, int orderID, HandleResultApi handleResultApi, HttpContext httpContext) =>
            {
                ServiceResult<Order> result = await getOrderUC.GetOrderByID(orderID);
                if (!result.IsSuccess)
                {
                    return handleResultApi.MapServiceResultToHttp(result);
                }

                if ((result.Item.CustomerID == TokenService.GetJWTClaim(httpContext)?.CustomerID) || TokenService.GetJWTClaim(httpContext).Role == "Admin")
                {
                    return handleResultApi.MapServiceResultToHttp(result);
                }

                return Results.Forbid();
            })
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy Order theo ID";
                operation.Description = "Lấy thông tin chi tiết của một đơn hàng dựa trên ID.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Orders" } };
                operation.Parameters[0].Description = "ID của đơn hàng.";
                return operation;
            });
        }

        public static void MapGetOrdetailOfOrder(this WebApplication app)
        {
            app.MapGet("/orders/{orderID}/order-details", async (GetOrderUC getOrderUC, int orderID, HandleResultApi handleResultApi, HttpContext httpContext) =>
            {
                ServiceResult<Order> resultSearch = await getOrderUC.GetOrderByID(orderID);
                if (!resultSearch.IsSuccess)
                {
                    return handleResultApi.MapServiceResultToHttp(resultSearch);
                }

                if ((resultSearch.Item.CustomerID == TokenService.GetJWTClaim(httpContext)?.CustomerID) || TokenService.GetJWTClaim(httpContext).Role == "Admin")
                {
                    ServiceResult<OrderDetail> result = await getOrderUC.GetOrderDetailOfOrder(resultSearch.Item);
                    return handleResultApi.MapServiceResultToHttp(result);
                }

                return Results.Forbid();
            })
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy các chi tiết đơn hàng (Order Details)";
                operation.Description = "Lấy thông tin chi tiết các mục sản phẩm (order details) trong một đơn hàng.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Orders" } };
                operation.Parameters[0].Description = "ID của đơn hàng.";
                return operation;
            });
        }
        #endregion

        #region Update Order USECASE
        public static void MapUpdateOrderUseCaseAPIs(this WebApplication app)
        {
            MapUpdateOrderStatus(app);
        }

        public static void MapUpdateOrderStatus(this WebApplication app)
        {
            app.MapPatch("/orders/{orderID}/status", async (UpdateOrderUC updateOrderUC, int orderID, [FromBody] string newStatus,
                HandleResultApi handleResultApi, IHubContext<OrderHub> OrderHubContext) =>
            {
                ServiceResult<Order> result = await updateOrderUC.UpdateStatusOrder(orderID, newStatus);
                if (result.IsSuccess)
                {
                    await OrderHubContext.Clients.All.SendAsync(
                        "OrderChanged",
                        result.Item,
                        "OrderUpdated"
                        );
                }
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Cập nhật trạng thái đơn hàng";
                operation.Description = "Cập nhật trạng thái của một đơn hàng cụ thể.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Orders" } };
                operation.Parameters[0].Description = "ID của đơn hàng.";
                return operation;
            });
        }
        #endregion 
    }
}
