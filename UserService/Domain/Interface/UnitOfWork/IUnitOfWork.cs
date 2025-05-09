using UserService.Domain.Entities;
using UserService.Domain.Interface.IRepositories;

namespace UserService.Domain.Interface.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Account> AccountRepository();
        IRepository<Customer> CustomerRepository();
        Task Commit();
        void Rollback();
    }
}
