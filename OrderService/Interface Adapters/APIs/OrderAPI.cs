using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Usecases;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.DBContext;

namespace OrderService.Interface_Adapters.API
{
    public static class OrderAPI
    {
        public static void MapOrderEndpoints(this WebApplication app)
        {
            MapCreateOrderUseCaseAPIs(app);
            MapGetOrderUsecaseAPIs(app);
            MapUpdateOrderUseCaseAPIs(app);
        }

        #region Create Order USECASE
        public static void MapCreateOrderUseCaseAPIs(this WebApplication app)
        {
            MapCreateOrderNormal(app);
        }

        public static void MapCreateOrderNormal(this WebApplication app)
        {
            app.MapPost("/orders", async (OrderContext orderContext, int CustomerID, List<OrderItem> orderItems) =>
            {
                try
                {
                    return Results.Ok(await new CreateOrderUC(orderContext).CreateOrder(CustomerID, orderItems));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
        }
        #endregion
        #region Get Order USECASE
        public static void MapGetOrderUsecaseAPIs(this WebApplication app)
        {
            MapGetOrdersByCustomer(app);
            MapGetOrderByID(app);
            MapGetAllOrders(app);
        }

        public static void MapGetOrdersByCustomer(this WebApplication app)
        {
            app.MapGet("/customers/{customerID}/orders", async (OrderContext orderContext, int customerID) => {
                return await new GetOrderUC(orderContext).GetByCustomerID(customerID);
            });
        }

        public static void MapGetOrderByID(this WebApplication app)
        {
            app.MapGet("/orders/{orderId}", async (OrderContext orderContext, int orderId) => {
                return await new GetOrderUC(orderContext).GetByOrderID(orderId);
            });
        }

        public static void MapGetAllOrders(this WebApplication app)
        {
            app.MapGet("/orders", async (OrderContext orderContext) => {
                return await new GetOrderUC(orderContext).GetAllOrder();
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
            app.MapPatch("/orders/{orderID}/status", async (OrderContext orderContext, int orderID, [FromBody] string newStatus) =>
            {
                return await new UpdateOrderUC(orderContext).UpdateStatusOrder(orderID, newStatus);
            });
        }
        #endregion
    }
}
