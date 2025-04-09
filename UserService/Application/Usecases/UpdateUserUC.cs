using Microsoft.EntityFrameworkCore;
using UserService.Application.UnitOfWorks;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Infrastructure.DBContext;

namespace UserService.Application.Usecases
{
    public class UpdateUserUC
    {
        private readonly IUnitOfWork unitOfWork;
        public UpdateUserUC(UserContext userContext)
        {
            this.unitOfWork = new UserUnitOfWork(userContext);
        }

        public async Task<Customer?> UpdateCustomerInformation(int customerID, Customer newCustomer)
        {
            try
            {
                IQueryable<Customer> query = this.unitOfWork.CustomerRepository().GetByIdQueryable(customerID);
                Customer? customer = await query.FirstOrDefaultAsync().ConfigureAwait(false);
                if (customer == null)
                {
                    return null;
                }

                customer.Phone = newCustomer.Phone;
                customer.Name = newCustomer.Name;

                this.unitOfWork.CustomerRepository().Update(customer);
                await unitOfWork.Commit().ConfigureAwait(false);

                return customer;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhât thông tin khách hàng" + ex.Message);
                return null;
            }
        }

        public async Task<Account?> UpdateAccount(int accountID, Account newAccount)
        {
            try
            {
                IQueryable<Account> query = this.unitOfWork.AccountRepository().GetByIdQueryable(accountID);
                Account? account = await query.FirstOrDefaultAsync().ConfigureAwait(false);
                if (account == null)
                {
                    return null;
                }
         
                account.Status = newAccount.Status;
                account.Role = newAccount.Role;
                account.Password = newAccount.Password;

                this.unitOfWork.AccountRepository().Update(account);
                await unitOfWork.Commit().ConfigureAwait(false);

                return account;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhât thông tin Account" + ex.Message);
                return null;
            }
        }
    }
}
