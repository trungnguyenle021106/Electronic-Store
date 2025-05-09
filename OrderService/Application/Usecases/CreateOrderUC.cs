using OrderService.Application.UnitOfWork;
using OrderService.Domain.Entities;
using OrderService.Domain.Interface.UnitOfWork;
using OrderService.Infrastructure.DBContext;

namespace OrderService.Application.Usecases
{
    public class CreateOrderUC
    {
        private readonly IUnitOfWork _UnitOfWork;

        public CreateOrderUC(OrderContext context)
        {
            this._UnitOfWork = new OrderUnitOfWork(context);
        }

        public async Task<Order?> CreateOrder(int CustomerID, List<OrderItem> listOrderItem)
        {
            DateTime OrderDate = DateTime.Now;
            float Total = listOrderItem.Sum(item => item.Price * item.Quantity);
            string Status = "Pending";
            try
            {
                Order order = await this._UnitOfWork.OrderRepository().Add(new Order
                {
                    CustomerID = CustomerID,
                    OrderDate = OrderDate,
                    Total = Total,
                    Status = Status
                });

                int OrderID = order.ID;
                List<Task> tasks = new List<Task>();

                foreach (var orderItem in listOrderItem)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderID = order.ID,
                        ProductID = orderItem.ID,
                        Quantity = orderItem.Quantity,
                        TotalPrice = orderItem.Price * orderItem.Quantity
                    };
                    tasks.Add(this._UnitOfWork.OrderDetailRepository().Add(orderDetail));
                }

                await Task.WhenAll(tasks);
                await this._UnitOfWork.Commit();

                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                this._UnitOfWork.Rollback();
                return null;
            }
        }
    }
}
