using CommonDto.ResultDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
                       "OrderChanged", // Tên event mà client Angular sẽ lắng nghe
                       result.Item,
                       "OrderAdded" // Thêm một tin nhắn kèm theo
                       );
                }

                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyCustomer");
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
            }).RequireAuthorization();
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
            }).RequireAuthorization();
        }

        public static void GetPagedOrders(this WebApplication app)
        {
            app.MapGet("/orders", async (GetOrderUC getOrderUC, [FromQuery] string? searchText, [FromQuery] bool? isIncrease,
                [FromQuery] int page, [FromQuery] int pageSize, HandleResultApi handleResultApi) =>
            {
                ServiceResult<PagedResult<Order>> result = await getOrderUC.GetPagedOrder(page, pageSize, searchText, isIncrease);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }   

        public static void MapGetOrdersByCustomerID(this WebApplication app)
        {
            app.MapGet("/customers/{customerID}/orders", async (GetOrderUC getOrderUC, int customerID, HandleResultApi handleResultApi) =>
            {
                ServiceResult<Order> result = await getOrderUC.GetOrdersByCustomerID(customerID);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyCustomer");
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
            }).RequireAuthorization();
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
            }).RequireAuthorization();
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
                       "OrderChanged", // Tên event mà client Angular sẽ lắng nghe
                       result.Item,
                       "OrderUpdated" // Thêm một tin nhắn kèm theo
                       );
                }
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }
        #endregion
    }
}
