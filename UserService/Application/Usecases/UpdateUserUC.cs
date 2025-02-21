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
                Customer customer = await this.unitOfWork.CustomerRepository().GetById(customerID);
                customer.Phone = newCustomer.Phone;
                customer.Name = newCustomer.Name;
                await this.unitOfWork.Commit();
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
                Account account = await this.unitOfWork.AccountRepository().GetById(accountID);
                account.Status = newAccount.Status;
                account.Role = newAccount.Role;
                account.Password = newAccount.Password;
                await this.unitOfWork.Commit();
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
