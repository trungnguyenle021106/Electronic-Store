using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Security.Claims;
using System.Text;
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

        public async Task<LoginResponse> LoginAccount(string userName, string passWord)
        {
            IQueryable<Account> accounts = (IQueryable<Account>)this.unitOfWork.AccountRepository().GetAll();
            Account? account = await (from a in accounts
                                      where a.UserName == userName
                                      select a).FirstOrDefaultAsync();
            if (account == null)
            {
                return new LoginResponse(0, "Tài khoản không hợp lệ", null, null, null, null);
            }

            if (!account.Password.Equals(passWord))
            {
                return new LoginResponse(2, "Sai mật khẩu", null, null, null, null);
            }

            return new LoginResponse(1, "Đăng nhập thành công", account.ID, account.Role, null, null);
        }
    }
}
