using OrderService.Domain.Entities;
using OrderService.Domain.Interface.IRepositories;

namespace OrderService.Domain.Interface.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Order> OrderRepository();
        IRepository<OrderDetail> OrderDetailRepository();
        Task Commit();
        void Rollback();
    }
}
