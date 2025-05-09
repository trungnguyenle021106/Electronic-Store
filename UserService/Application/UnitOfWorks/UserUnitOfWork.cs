using UserService.Domain.Entities;
using UserService.Domain.Interface.IRepositories;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Infrastructure.DBContext;
using UserService.Infrastructure.Repository;

namespace UserService.Application.UnitOfWorks
{
    public class UserUnitOfWork : IUnitOfWork
    {
        private readonly UserContext _Context;
        private readonly IRepository<Account> _AccountRepository;
        private readonly IRepository<Customer> _CustomerRepository;
        public UserUnitOfWork(UserContext context)
        {
            _AccountRepository = new Repository<Account>(context);
            _CustomerRepository = new Repository<Customer>(context);
            _Context = context;
        }

        public async Task Commit()
        {
            await this._Context.SaveChangesAsync();
        }

        public void Rollback()
        {
            if (_Context.Database.CurrentTransaction != null)
            {
                _Context.Database.CurrentTransaction.Rollback();
            }
        }

        public IRepository<Customer> CustomerRepository()
        {
            return _CustomerRepository;
        }

        public IRepository<Account> AccountRepository()
        {
            return _AccountRepository;
        }
    }
}
