using UserService.Application.UnitOfWorks;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Infrastructure.DBContext;

namespace UserService.Application.Usecases
{
    public class CreateUserUC
    {
        private readonly IUnitOfWork unitOfWork;
        public CreateUserUC(UserContext userContext)
        {
            this.unitOfWork = new UserUnitOfWork(userContext);
        }

        public async Task<Account?> CreateAccount(Account account)
        {
            try
            {
                Account newAccount = await this.unitOfWork.AccountRepository().Add(account);
                await this.unitOfWork.Commit();
                return newAccount;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi tạo tài khoản :" + ex);
                return null;
            }
        }

        public async Task<Customer?> CreateCustomer(Customer customer)
        {
            try
            {
                Customer newCustomer = await this.unitOfWork.CustomerRepository().Add(customer);
                await this.unitOfWork.Commit();
                return newCustomer;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi tạo thông tin khách hàng :" + ex);
                return null;
            }
        }
    }
}
