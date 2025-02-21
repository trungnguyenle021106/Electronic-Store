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
                return await this.unitOfWork.AccountRepository().GetById(accountID);
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
                return await this.unitOfWork.CustomerRepository().GetById(customerID);
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
                return (List<Customer>)await this.unitOfWork.CustomerRepository().GetAll();
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
                return (List<Account>) await this.unitOfWork.AccountRepository().GetAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi lấy danh sách tài khoản " + ex.ToString());
                return null;
            }
        }
    }
}
