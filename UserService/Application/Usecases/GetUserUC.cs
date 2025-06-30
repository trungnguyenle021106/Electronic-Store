using ApiDto.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using UserService.Application.UnitOfWorks;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;


namespace UserService.Application.Usecases
{
    public class GetUserUC
    {
        private readonly IUnitOfWork unitOfWork;
        public GetUserUC(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<QueryResult<Account>> GetAccountByID(int accountID)
        {
            try
            {
                if (accountID <= 0)
                {
                    return QueryResult<Account>.Failure("Account ID is invalid.", RetrievalErrorType.ValidationError);
                }
                Account? account = await this.unitOfWork.AccountRepository().GetById(accountID).ConfigureAwait(false);

                if (account == null)
                {
                    return QueryResult<Account>.Failure("Account is not found.", RetrievalErrorType.NotFound);
                }

                return QueryResult<Account>.Success(account);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy account có id : {accountID}, lỗi : {ex.Message}");
                return QueryResult<Account>.Failure("An unexpected internal error occurred while get account.",
                      RetrievalErrorType.InternalError);
            }
        }

        public async Task<QueryResult<Customer>> GetCustomerByID(int customerID)
        {
            try
            {
                if (customerID <= 0)
                {
                    return QueryResult<Customer>.Failure("Customer ID is invalid.", RetrievalErrorType.ValidationError);
                }
                Customer? customer = await this.unitOfWork.CustomerRepository().GetById(customerID).ConfigureAwait(false);

                if (customer == null)
                {
                    return QueryResult<Customer>.Failure("Customer is not found.", RetrievalErrorType.NotFound);
                }

                return QueryResult<Customer>.Success(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy customer có id : {customerID}, lỗi : {ex.Message}");
                return QueryResult<Customer>.Failure("An unexpected internal error occurred while get customer.",
                      RetrievalErrorType.InternalError);
            }
        }

        public async Task<QueryResult<Customer>> GetCustomerByAccountID(int accountID)
        {
            try
            {
                if (accountID <= 0)
                {
                    return QueryResult<Customer>.Failure("Account is invalid.", RetrievalErrorType.ValidationError);
                }
                IQueryable<Account> accountQuery = this.unitOfWork.AccountRepository().GetAll();
                Customer? customer = await accountQuery.
                Join(
                    unitOfWork.CustomerRepository().GetAll(),
                    account => account.ID,
                    customer => customer.AccountID,
                    (account, customer) => new { Account = account, Customer = customer }
                )
                .Where(joined => joined.Account.ID == accountID)
                .Select(joined => joined.Customer)
                .FirstOrDefaultAsync().ConfigureAwait(false);

                if (customer == null)
                {
                    return QueryResult<Customer>.Failure("Customer is not found.", RetrievalErrorType.NotFound);
                }

                return QueryResult<Customer>.Success(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy customer có accountID : {accountID}, lỗi : {ex.Message}");
                return QueryResult<Customer>.Failure("An unexpected internal error occurred while get customer.",
                      RetrievalErrorType.InternalError);
            }
        }

        public async Task<QueryResult<Customer>> GetAllCustomer()
        {
            try
            {
                IQueryable<Customer> customersQuery = this.unitOfWork.CustomerRepository().GetAll();
                List<Customer> customersList = await customersQuery.ToListAsync().ConfigureAwait(false);
                return QueryResult<Customer>.Success(customersList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách customer : {ex.Message}");
                return QueryResult<Customer>.Failure("An unexpected internal error occurred while get customers.",
                      RetrievalErrorType.InternalError);
            }
        }

        public async Task<QueryResult<Account>> GetAllAccount()
        {
            try
            {
                IQueryable<Account> accountsQuery = this.unitOfWork.AccountRepository().GetAll();
                List<Account> accountsList = await accountsQuery.ToListAsync().ConfigureAwait(false);

                return QueryResult<Account>.Success(accountsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách account : {ex.Message}");
                return QueryResult<Account>.Failure("An unexpected internal error occurred while get accounts.",
                      RetrievalErrorType.InternalError);
            }
        }
    }
}
