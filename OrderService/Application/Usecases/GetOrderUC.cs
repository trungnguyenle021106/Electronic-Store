using Microsoft.EntityFrameworkCore;
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
                IQueryable<Order> query = this._UnitOfWork.OrderRepository().GetByIdQueryable(id);
                Order order = await query.FirstOrDefaultAsync();

                if (order == null) { return null; }
                return order;
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
                IQueryable<Order> query = this._UnitOfWork.OrderRepository().GetAll();
                List<Order> listOrder = await query.ToListAsync();
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
                IQueryable<Order> query = this._UnitOfWork.OrderRepository().GetAll();
                List<Order> listOrder = await query.ToListAsync();

                if(listOrder == null) { return null; }
                return listOrder;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách Order : {ex.Message}");
                return null;
            }
        }
    }
}
