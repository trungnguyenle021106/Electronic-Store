using Microsoft.EntityFrameworkCore;
using UserService.Application.UnitOfWorks;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Infrastructure.DBContext;

namespace UserService.Application.Usecases
{
    public class CreateUserUC
    {
        private readonly IUnitOfWork unitOfWork;
        public CreateUserUC(IUnitOfWork userUnitOfWork)
        {
            this.unitOfWork = userUnitOfWork;
        }
        
        public async Task<Account?> CreateAccount(Account account)
        {
            try
            {
                IQueryable<Account> accounts = this.unitOfWork.AccountRepository().GetAll();
                Account? newAccount = await (from a in accounts
                                             where a.Email == account.Email
                                             select a).FirstOrDefaultAsync().ConfigureAwait(false);
                if (newAccount == null)
                {
                    await this.unitOfWork.AccountRepository().Add(account).ConfigureAwait(false);
                    await this.unitOfWork.Commit().ConfigureAwait(false);

                    Account? createdAccount = await (from a in accounts
                                                 where a.Email == account.Email
                                                 select a).FirstOrDefaultAsync().ConfigureAwait(false) ;
                    return createdAccount;
                }
                return null;
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

                Customer newCustomer = await this.unitOfWork.CustomerRepository().Add(customer).ConfigureAwait(false);
                await this.unitOfWork.Commit().ConfigureAwait(false);
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
