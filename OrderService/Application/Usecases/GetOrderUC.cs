using OrderService.Application.UnitOfWork;
using OrderService.Domain.Entities;
using OrderService.Domain.Interface.UnitOfWork;
using OrderService.Infrastructure.DBContext;

namespace OrderService.Application.Usecases
{
    public class GetOrderUC
    {
        private readonly IUnitOfWork _UnitOfWork;

        public GetOrderUC(OrderContext context)
        {
            this._UnitOfWork = new OrderUnitOfWork(context);
        }

        public async Task<Order?> GetByOrderID(int id)
        {
            try
            {
                return await this._UnitOfWork.OrderRepository().GetById(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy Order theo ID {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Order>?> GetByCustomerID(int id)
        {
            try
            {
                List<Order> listOrder = (List<Order>)await this._UnitOfWork.OrderRepository().GetAll();
                return listOrder.Where(order => order.CustomerID == id).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách Order theo Customer ID {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Order>?> GetAllOrder()
        {
            try
            {
                return (List<Order>)await this._UnitOfWork.OrderRepository().GetAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách Order : {ex.Message}");
                return null;
            }
        }
    }
}
