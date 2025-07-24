using CommonDto;
using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Service;
using UserService.Domain.DTO;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;

namespace UserService.Application.Usecases
{
    public class CreateUserUC
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly TokenService tokenService;
        private readonly HashService hashService;

        public CreateUserUC(IUnitOfWork userUnitOfWork, TokenService tokenService, HashService hashService)
        {
            this.unitOfWork = userUnitOfWork;
            this.tokenService = tokenService;
            this.hashService = hashService;
        }

        public async Task<ServiceResult<Account>> CreateAccount(Account account)
        {
            try
            {
                if (account == null)
                {
                    return ServiceResult<Account>
                       .Failure("No account provided to add.", ServiceErrorType.ValidationError);
                }

                IQueryable<Account> query = this.unitOfWork.AccountRepository().GetAll();
                query = query.Where(a => a.Email == account.Email);
                Account? newAccount = await query.FirstOrDefaultAsync().ConfigureAwait(false);

                if (newAccount != null)
                {
                    return ServiceResult<Account>.Failure("Account is exist", ServiceErrorType.AlreadyExists);
                }

                await this.unitOfWork.AccountRepository().Add(account).ConfigureAwait(false);
                await this.unitOfWork.Commit().ConfigureAwait(false);
                return ServiceResult<Account>.Success(account);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating account: {ex}");

                return ServiceResult<Account>.Failure(
                    "An unexpected internal error occurred during product creation.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<Customer>> CreateCustomer(Customer customer)
        {
            try
            {
                if (customer == null)
                {
                    return ServiceResult<Customer>
                       .Failure("No customer provided to add.", ServiceErrorType.ValidationError);
                }

                IQueryable<Customer> query = this.unitOfWork.CustomerRepository().GetAll();
                query = query.Where(c => c.AccountID == customer.AccountID);

                Customer? newCustomer = await query.FirstOrDefaultAsync().ConfigureAwait(false);
                if (newCustomer != null)
                {
                    return ServiceResult<Customer>.Failure("Customer is exist", ServiceErrorType.AlreadyExists);
                }

                await this.unitOfWork.CustomerRepository().Add(customer).ConfigureAwait(false);
                await this.unitOfWork.Commit().ConfigureAwait(false);
                return ServiceResult<Customer>.Success(customer);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating account: {ex}");

                return ServiceResult<Customer>.Failure(
                    "An unexpected internal error occurred during product creation.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<RefreshToken>> CreateRefreshToken(int accountID)
        {
            try
            {
                if (accountID <= 0)
                {
                    return ServiceResult<RefreshToken>
                    .Failure("No account provided to add.", ServiceErrorType.ValidationError);
                }

                IQueryable<Account> query = this.unitOfWork.AccountRepository().GetAll();
                query = query.Where(a => a.ID == accountID);
                Account? account = await query.FirstOrDefaultAsync().ConfigureAwait(false);

                if (account == null)
                {
                    return ServiceResult<RefreshToken>
                  .Failure("No account provided to add.", ServiceErrorType.ValidationError);
                }

                RefreshToken refreshToken = new RefreshToken
                {
                    TokenHash = hashService.Hash(tokenService.GenerateRefreshToken()),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    RevokedAt = null,
                    AccountID = accountID,
                    IsRevoked = false
                };
                await this.unitOfWork.RefreshTokenRepository().Add(refreshToken).ConfigureAwait(false);
                await this.unitOfWork.Commit().ConfigureAwait(false);
                return ServiceResult<RefreshToken>.Success(refreshToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating account: {ex}");

                return ServiceResult<RefreshToken>.Failure(
                    "An unexpected internal error occurred during product creation.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<string>> RefreshAccessToken(RefreshToken refreshToken)
        {
            int accountID = refreshToken?.AccountID ?? 0;
            if (accountID <= 0)
            {
                return ServiceResult<string>
                .Failure("No refresh token provided to add.", ServiceErrorType.ValidationError);
            }

            if (refreshToken.IsRevoked == true || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return ServiceResult<string>
             .Failure("Refresh token is expired or revoked.", ServiceErrorType.Invalid);
            }
            try
            {
                int? customerID = this.unitOfWork.CustomerRepository()
                    .GetAll()
                    .Where(c => c.AccountID == accountID)
                    .Select(c => c.ID)
                    .FirstOrDefault();
                Account account = await unitOfWork.AccountRepository().GetById(accountID).ConfigureAwait(false);
                string accessToken = this.tokenService.GenerateAccessToken(new JWTClaim(account.ID, account.Role, customerID));
           
                return ServiceResult<string>.Success(accessToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating JWT: {ex}");

                return ServiceResult<string>.Failure(
                    "An unexpected internal error occurred during JWT creation.",
                    ServiceErrorType.InternalError
                );
            }
        }
    }
}
