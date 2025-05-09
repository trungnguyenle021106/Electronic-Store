using Microsoft.EntityFrameworkCore;
using System.Data;
using UserService.Application.UnitOfWorks;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Domain.Response;
using UserService.Infrastructure.DBContext;

namespace UserService.Application.Usecases
{
    public class LoginLogoutSignUpUC
    {
        private readonly IUnitOfWork unitOfWork;
        public LoginLogoutSignUpUC(UserContext userContext)
        {
            this.unitOfWork = new UserUnitOfWork(userContext);
        }

        public async Task<LoginResponse> LoginAccount(string email, string passWord)
        {
            IQueryable<Account> accounts = this.unitOfWork.AccountRepository().GetAll();
            Account? account = await (from a in accounts
                                      where a.Email == email
                                      select a).FirstOrDefaultAsync().ConfigureAwait(false);

            if (account == null)
            {
                return new LoginResponse(0, "Tài khoản không hợp lệ", null, null, null);
            }

            if (!account.Password.Equals(passWord))
            {
                return new LoginResponse(2, "Sai mật khẩu", null, null, null);
            }

            IQueryable<Customer> customers = this.unitOfWork.CustomerRepository().GetAll();
            Customer? customer = await (from c in customers
                                        where c.AccountID == account.ID
                                        select c).FirstOrDefaultAsync().ConfigureAwait(false);

            return new LoginResponse(1, "Đăng nhập thành công", account.ID, account.Role, customer);
        }
    }
}
