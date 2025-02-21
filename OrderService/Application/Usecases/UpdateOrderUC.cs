using OrderService.Application.UnitOfWork;
using OrderService.Domain.Entities;
using OrderService.Domain.Interface.UnitOfWork;
using OrderService.Infrastructure.DBContext;

namespace OrderService.Application.Usecases
{
    public class UpdateOrderUC
    {
        private readonly IUnitOfWork unitOfWork;

        public UpdateOrderUC(OrderContext context)
        {
            this.unitOfWork = new OrderUnitOfWork(context);
        }

        public async Task<Order?> UpdateStatusOrder(int orderID, string status)
        {
            try
            {
                Order order = await this.unitOfWork.OrderRepository().GetById(orderID);
                order.Status = status;  
                this.unitOfWork.OrderRepository().Update(order);
                await this.unitOfWork.Commit();
                return order;
            }
            catch (Exception ex) {
                Console.WriteLine($"Lỗi cập nhật Order: {ex.Message}");
                return null;
            }
        }
    }
}
