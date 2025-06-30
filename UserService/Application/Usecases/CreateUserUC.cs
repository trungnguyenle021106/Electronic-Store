using ApiDto.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using UserService.Application.Service;
using UserService.Application.UnitOfWorks;
using UserService.Domain.DTO;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Domain.Request;


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

        public async Task<CreationResult<Account>> CreateAccount(Account account)
        {
            try
            {
                if (account == null)
                {
                    return CreationResult<Account>
                       .Failure("No account provided to add.", CreationErrorType.ValidationError);
                }

                IQueryable<Account> query = this.unitOfWork.AccountRepository().GetAll();
                query = query.Where(a => a.Email == account.Email);
                Account? newAccount = await query.FirstOrDefaultAsync().ConfigureAwait(false);

                // Cách viết theo query syntax
                //Account ? newAccount = await (from a in accounts
                //                             where a.Email == account.Email
                //                             select a).FirstOrDefaultAsync().ConfigureAwait(false);

                if (newAccount != null)
                {
                    return CreationResult<Account>.Failure("Account is exist", CreationErrorType.AlreadyExists);
                }

                await this.unitOfWork.AccountRepository().Add(account).ConfigureAwait(false);
                await this.unitOfWork.Commit().ConfigureAwait(false);
                return CreationResult<Account>.Success(account);
            }
            catch (Exception ex)
            {
                // Bắt các lỗi hệ thống không mong muốn khác (lỗi database, lỗi mạng, lỗi logic, ...)
                Console.Error.WriteLine($"Error creating account: {ex}");

                return CreationResult<Account>.Failure(
                    "An unexpected internal error occurred during product creation.",
                    CreationErrorType.InternalError
                );
            }
        }

        public async Task<CreationResult<Customer>> CreateCustomer(Customer customer)
        {
            try
            {
                if (customer == null)
                {
                    return CreationResult<Customer>
                       .Failure("No customer provided to add.", CreationErrorType.ValidationError);
                }

                IQueryable<Customer> query = this.unitOfWork.CustomerRepository().GetAll();
                query = query.Where(c => c.AccountID == customer.AccountID);

                Customer? newCustomer = await query.FirstOrDefaultAsync().ConfigureAwait(false);
                if (newCustomer != null)
                {
                    return CreationResult<Customer>.Failure("Customer is exist", CreationErrorType.AlreadyExists);
                }

                await this.unitOfWork.CustomerRepository().Add(customer).ConfigureAwait(false);
                await this.unitOfWork.Commit().ConfigureAwait(false);
                return CreationResult<Customer>.Success(customer);
            }
            catch (Exception ex)
            {
                // Bắt các lỗi hệ thống không mong muốn khác (lỗi database, lỗi mạng, lỗi logic, ...)
                Console.Error.WriteLine($"Error creating account: {ex}");

                return CreationResult<Customer>.Failure(
                    "An unexpected internal error occurred during product creation.",
                    CreationErrorType.InternalError
                );
            }
        }

        public async Task<CreationResult<RefreshToken>> CreateRefreshToken(int accountID)
        {
            try
            {
                if (accountID <= 0)
                {
                    return CreationResult<RefreshToken>
                    .Failure("No account provided to add.", CreationErrorType.ValidationError);
                }

                IQueryable<Account> query = this.unitOfWork.AccountRepository().GetAll();
                query = query.Where(a => a.ID == accountID);
                Account? account = await query.FirstOrDefaultAsync().ConfigureAwait(false);

                if (account == null)
                {
                    return CreationResult<RefreshToken>
                  .Failure("No account provided to add.", CreationErrorType.ValidationError);
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
                return CreationResult<RefreshToken>.Success(refreshToken);
            }
            catch (Exception ex)
            {
                // Bắt các lỗi hệ thống không mong muốn khác (lỗi database, lỗi mạng, lỗi logic, ...)
                Console.Error.WriteLine($"Error creating account: {ex}");

                return CreationResult<RefreshToken>.Failure(
                    "An unexpected internal error occurred during product creation.",
                    CreationErrorType.InternalError
                );
            }
        }

        public async Task<CreationResult<string>> RefreshAccessToken(RefreshToken refreshToken)
        {
            try
            {
                int idAccount = refreshToken?.AccountID ?? 0;
                if (idAccount <= 0)
                {
                    return CreationResult<string>
                    .Failure("No refresh token provided to add.", CreationErrorType.ValidationError);
                }

                if(refreshToken.IsRevoked == true ||  refreshToken.ExpiresAt < DateTime.UtcNow)
                {
                    return CreationResult<string>
                 .Failure("Refresh token is expired or revoked.", CreationErrorType.Invalid);
                }

                Account account = await unitOfWork.AccountRepository().GetById(idAccount).ConfigureAwait(false);
                string accessToken = this.tokenService.GenerateAccessToken(new JWTClaim(account.ID, account.Role));
           
                return CreationResult<string>.Success(accessToken);
            }
            catch (Exception ex)
            {
                // Bắt các lỗi hệ thống không mong muốn khác (lỗi database, lỗi mạng, lỗi logic, ...)
                Console.Error.WriteLine($"Error creating JWT: {ex}");

                return CreationResult<string>.Failure(
                    "An unexpected internal error occurred during JWT creation.",
                    CreationErrorType.InternalError
                );
            }
        }
    }
}
