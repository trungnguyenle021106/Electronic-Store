using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using UserService.Application.UnitOfWorks;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Infrastructure.DBContext;

namespace UserService.Application.Usecases
{
    public class GetUserUC
    {
        private readonly IUnitOfWork unitOfWork;
        public GetUserUC(UserContext userContext)
        {
            this.unitOfWork = new UserUnitOfWork(userContext);
        }

        public async Task<Account?> GetAccountByID(int accountID)
        {
            try
            {
                IQueryable<Account> query = this.unitOfWork.AccountRepository().GetByIdQueryable(accountID);
                Account? account = await query.FirstOrDefaultAsync().ConfigureAwait(false);
                if (account == null)
                {
                    return null;
                }

                return account;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi lấy tài khoản id : " + accountID + " lỗi : " + ex.ToString());
                return null;
            }
        }

        public async Task<Customer?> GetCustomerByID(int customerID)
        {
            try
            {
                IQueryable<Customer> query = this.unitOfWork.CustomerRepository().GetByIdQueryable(customerID);
                Customer? customer = await query.FirstOrDefaultAsync().ConfigureAwait(false);
                if (customer == null)
                {
                    return null;
                }
                return customer;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi lấy khách hàng id : " + customerID + " lỗi : " + ex.ToString());
                return null;
            }
        }

        public async Task<List<Customer>?> GetAllCustomer()
        {
            try
            {
                IQueryable<Customer> customersQuery = this.unitOfWork.CustomerRepository().GetAll();
                List<Customer> customersList = await customersQuery.ToListAsync().ConfigureAwait(false);
                return customersList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi lấy danh sách khách hàng " + ex.ToString());
                return null;
            }
        }

        public async Task<List<Account>?> GetAllAccount()
        {
            try
            {
                IQueryable<Account> accountsQuery = this.unitOfWork.AccountRepository().GetAll();
                List<Account> accountsList = await accountsQuery.ToListAsync().ConfigureAwait(false);
                return accountsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi lấy danh sách tài khoản " + ex.ToString());
                return null;
            }
        }
    }
}
