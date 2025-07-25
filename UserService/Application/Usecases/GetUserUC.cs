using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Domain.Response;

namespace UserService.Application.Usecases
{
    public class GetUserUC
    {
        private readonly IUnitOfWork unitOfWork;
        public GetUserUC(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }


        public async Task<ServiceResult<PagedResult<Account>>> GetPagedAccount(int page, int pageSize, string? searchText)
        {
            try
            {
                IQueryable<Account> query = this.unitOfWork.AccountRepository().GetAll();

                // 1. Áp dụng tìm kiếm (Search) vào tất cả các cột có thể
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string searchLower = searchText.Trim().ToLower();

                    query = query.Where(acc =>
                        (acc.Status != null && acc.Status.ToLower().Contains(searchLower)) ||
                        (acc.Email.ToLower().Contains(searchLower)) ||
                        (acc.Role.ToLower().Contains(searchLower))
                    );
                }

           

                int totalCount = await query.CountAsync();

                List<Account> list = new List<Account>();
                // 4. Kiểm tra trang hợp lệ

                if (page < 1)
                {
                    page = 1;
                }
                query = query.OrderBy(pp => pp.ID); // Sắp xếp mặc định theo ID

                // 6. Áp dụng phân trang (Skip và Take)
                list = await query
                   .Skip(((page - 1) * pageSize))
                   .Take( pageSize)
                   .ToListAsync().ConfigureAwait(false);

                return ServiceResult<PagedResult<Account>>.Success(new PagedResult<Account>
                {
                    Items = list,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount // Sử dụng totalCount đã được lọc/tìm kiếm
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách Order, lỗi : {ex.Message}");
                return ServiceResult<PagedResult<Account>>.Failure(
                    "An error occurred while retrieving the orders.",
                    ServiceErrorType.InternalError);
            }
        }


        public async Task<ServiceResult<Account>> GetAccountByID(int accountID)
        {
            try
            {
                if (accountID <= 0)
                {
                    return ServiceResult<Account>.Failure("Account ID is invalid.", ServiceErrorType.ValidationError);
                }
                Account? account = await this.unitOfWork.AccountRepository().GetById(accountID).ConfigureAwait(false);

                if (account == null)
                {
                    return ServiceResult<Account>.Failure("Account is not found.", ServiceErrorType.NotFound);
                }

                return ServiceResult<Account>.Success(account);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy account có id : {accountID}, lỗi : {ex.Message}");
                return ServiceResult<Account>.Failure("An unexpected internal error occurred while get account.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Customer>> GetCustomerByID(int customerID)
        {
            try
            {
                if (customerID <= 0)
                {
                    return ServiceResult<Customer>.Failure("Customer ID is invalid.", ServiceErrorType.ValidationError);
                }
                Customer? customer = await this.unitOfWork.CustomerRepository().GetById(customerID).ConfigureAwait(false);

                if (customer == null)
                {
                    return ServiceResult<Customer>.Failure("Customer is not found.", ServiceErrorType.NotFound);
                }

                return ServiceResult<Customer>.Success(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy customer có id : {customerID}, lỗi : {ex.Message}");
                return ServiceResult<Customer>.Failure("An unexpected internal error occurred while get customer.",
                      ServiceErrorType.InternalError);
            }
        }


        public async Task<ServiceResult<CustomerInformation>> GetCustomerInformationByCustomerID(int customerID)
        {
            if (customerID <= 0)
            {
                return ServiceResult<CustomerInformation>.Failure("Customer ID is invalid.", ServiceErrorType.ValidationError);
            }

            try
            {
                Customer? customer = await this.unitOfWork.CustomerRepository().GetById(customerID).ConfigureAwait(false);
                if (customer == null)
                {
                    return ServiceResult<CustomerInformation>.Failure("Customer is not found.", ServiceErrorType.NotFound);
                }
                string email = (await this.unitOfWork.AccountRepository().GetById(customer.AccountID ?? 0).ConfigureAwait(false)).Email;
                return ServiceResult<CustomerInformation>.Success(new CustomerInformation(customer.ID, email, customer.Name, customer.Phone, customer.Address, customer.Gender));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy customer có id : {customerID}, lỗi : {ex.Message}");
                return ServiceResult<CustomerInformation>.Failure("An unexpected internal error occurred while get customer.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<CustomerInformation>> GetCustomerInformationByAccountID(int accountID)
        {
            if (accountID <= 0)
            {
                return ServiceResult<CustomerInformation>.Failure("Account is invalid.", ServiceErrorType.ValidationError);
            }
            try
            {          
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

                string email = (await this.unitOfWork.AccountRepository().GetById(accountID).ConfigureAwait(false)).Email;
                if (customer == null)
                {
                    return ServiceResult<CustomerInformation>.Failure("Customer is not found.", ServiceErrorType.NotFound);
                }

                return ServiceResult<CustomerInformation>.Success(new CustomerInformation(customer.ID, email, customer.Name, customer.Phone, customer.Address, customer.Gender));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy customer có accountID : {accountID}, lỗi : {ex.Message}");
                return ServiceResult<CustomerInformation>.Failure("An unexpected internal error occurred while get customer.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Customer>> GetAllCustomer()
        {
            try
            {
                IQueryable<Customer> customersQuery = this.unitOfWork.CustomerRepository().GetAll();
                List<Customer> customersList = await customersQuery.ToListAsync().ConfigureAwait(false);
                return ServiceResult<Customer>.Success(customersList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách customer : {ex.Message}");
                return ServiceResult<Customer>.Failure("An unexpected internal error occurred while get customers.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Account>> GetAllAccount()
        {
            try
            {
                IQueryable<Account> accountsQuery = this.unitOfWork.AccountRepository().GetAll();
                List<Account> accountsList = await accountsQuery.ToListAsync().ConfigureAwait(false);

                return ServiceResult<Account>.Success(accountsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách account : {ex.Message}");
                return ServiceResult<Account>.Failure("An unexpected internal error occurred while get accounts.",
                      ServiceErrorType.InternalError);
            }
        }
    }
}
