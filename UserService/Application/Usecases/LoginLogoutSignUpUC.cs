using CommonDto;
using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;
using System.Data;
using UserService.Application.Service;
using UserService.Domain.DTO;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Domain.Request;
using UserService.Infrastructure.Verify_Email;

namespace UserService.Application.Usecases
{
    public class LoginLogoutSignUpUC
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly TokenService tokenService;
        private readonly HashService hashService;
        private readonly EmailValidator emailValidator;

        public LoginLogoutSignUpUC(IUnitOfWork unitOfWork, TokenService tokenService, HashService hashService,
            EmailValidator emailValidator)
        {
            this.unitOfWork = unitOfWork;
            this.tokenService = tokenService;
            this.hashService = hashService;
            this.emailValidator = emailValidator;
        }

        public async Task<ServiceResult<LoginSignUpDTO>> LoginAccount(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return ServiceResult<LoginSignUpDTO>.Failure("Email và mật khẩu không được để trống.", ServiceErrorType.ValidationError);
            }

            try
            {
                Account? account = await this.unitOfWork.AccountRepository()
                                                    .GetAll()
                                                    .FirstOrDefaultAsync(a => a.Email == email)
                                                    .ConfigureAwait(false);

                if (account == null)
                {
                    return ServiceResult<LoginSignUpDTO>.Failure("Email hoặc mật khẩu không đúng.", ServiceErrorType.NotFound);
                }

                if (!hashService.VerifyHash(password, account.Password))
                {
                    return ServiceResult<LoginSignUpDTO>.Failure("Email hoặc mật khẩu không đúng.", ServiceErrorType.InvalidCredentials);
                }

                if (account.Status == "Locked")
                {
                    return ServiceResult<LoginSignUpDTO>.Failure("Tài khoản của bạn bị khóa.", ServiceErrorType.AccountLocked);
                }

                int? customerId = this.unitOfWork.CustomerRepository()
                    .GetAll()
                    .Where(c => c.AccountID == account.ID)
                    .Select(c => c.ID)
                    .FirstOrDefault();

                string accessToken = tokenService.GenerateAccessToken(new JWTClaim
                (
                    account.ID, account.Role, customerId
                ));
                string refreshTokenString = tokenService.GenerateRefreshToken();

                string hashRefreshTokenString = hashService.Hash(refreshTokenString);

                RefreshToken refreshToken = await this.unitOfWork.RefreshTokenRepository().Add(new RefreshToken
                {
                    TokenHash = hashRefreshTokenString,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    Account = account,
                    IsRevoked = false
                }).ConfigureAwait(false);

                await this.unitOfWork.Commit().ConfigureAwait(false);
                return ServiceResult<LoginSignUpDTO>.Success(new LoginSignUpDTO
                (
                    account, accessToken, refreshToken
                ));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred during login for email: {email}, error : {ex}");
                return ServiceResult<LoginSignUpDTO>.Failure(
                    "An unexpected internal error occurred during login .",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<LoginSignUpDTO>> SignUp(SignUpRequest signUpRequest)
        {
            if (signUpRequest == null)
            {
                return ServiceResult<LoginSignUpDTO>
                   .Failure("No sign up information provided to add.", ServiceErrorType.ValidationError);
            }

            if (!(await emailValidator.CheckEmailValid(signUpRequest.Email).ConfigureAwait(false)).Status)
            {
                return ServiceResult<LoginSignUpDTO>
                 .Failure("Email is not valid.", ServiceErrorType.ValidationError);
            }
            try
            {

                IQueryable<Account> queryAccount = this.unitOfWork.AccountRepository().GetAll();
                queryAccount = queryAccount.Where(a => a.Email == signUpRequest.Email);

                Account? account = await queryAccount.FirstOrDefaultAsync().ConfigureAwait(false);
                if (account != null)
                {
                    return ServiceResult<LoginSignUpDTO>.Failure("Account is exist", ServiceErrorType.AlreadyExists);
                }

                using (var transaction = await this.unitOfWork.BeginTransactionAsync().ConfigureAwait(false))
                {
                    try
                    {
                        Account newAccount = new Account
                        {
                            Email = signUpRequest.Email,
                            Password = hashService.Hash(signUpRequest.Password),
                            Role = "Customer",
                            Status = "Active"
                        };
                        await this.unitOfWork.AccountRepository().Add(newAccount).ConfigureAwait(false);
                        await this.unitOfWork.Commit().ConfigureAwait(false);

                        IQueryable<Customer> queryCustomer = this.unitOfWork.CustomerRepository().GetAll();
                        queryCustomer = queryCustomer.Where(a => a.AccountID == newAccount.ID);

                        Customer? customer = await queryCustomer.FirstOrDefaultAsync().ConfigureAwait(false);
                        if (customer != null)
                        {
                            return ServiceResult<LoginSignUpDTO>.Failure("Customer is exist", ServiceErrorType.AlreadyExists);
                        }

                        Customer newCustomer = new Customer
                        {
                            Phone = signUpRequest.Phone,
                            Address = signUpRequest.Address,
                            Name = signUpRequest.Name,
                            Gender = signUpRequest.Gender,
                            Account = newAccount,
                        };
                        await this.unitOfWork.CustomerRepository().Add(newCustomer).ConfigureAwait(false);
                        await this.unitOfWork.Commit().ConfigureAwait(false);

                        string refreshTokenString = tokenService.GenerateRefreshToken();
                        string hashRefreshTokenString = hashService.Hash(refreshTokenString);

                        RefreshToken refreshToken = await this.unitOfWork.RefreshTokenRepository().Add(new RefreshToken
                        {
                            TokenHash = hashRefreshTokenString,
                            ExpiresAt = DateTime.UtcNow.AddDays(7),
                            CreatedAt = DateTime.UtcNow,
                            Account = newAccount,
                            IsRevoked = false
                        }).ConfigureAwait(false);

                        await this.unitOfWork.Commit().ConfigureAwait(false);
                        await this.unitOfWork.CommitTransactionAsync(transaction).ConfigureAwait(false);

                        int? customerId = this.unitOfWork.CustomerRepository()
                          .GetAll()
                          .Where(c => c.AccountID == newAccount.ID)
                          .Select(c => c.ID)
                          .FirstOrDefault();
                        string accessToken = tokenService.GenerateAccessToken(new JWTClaim(newAccount.ID, newAccount.Role, customerId));

                        return ServiceResult<LoginSignUpDTO>.Success(new LoginSignUpDTO
                        (
                           newAccount, accessToken, refreshToken
                        ));
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"An unexpected error occurred during sign up, error : {ex}");
                        await this.unitOfWork.RollbackAsync(transaction).ConfigureAwait(false);
                        return ServiceResult<LoginSignUpDTO>.Failure(
                            "An unexpected internal error occurred during sign up .",
                            ServiceErrorType.InternalError
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error sign up : {ex}");

                return ServiceResult<LoginSignUpDTO>.Failure(
                    "An unexpected internal error occurred during sign up .",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<RefreshToken>> LogoutSpecificSession(HttpContext httpContext)
        {
            try
            {
                JWTClaim? jWTClaim = tokenService.GetJWTClaim(httpContext);
                RefreshToken? refreshToken = await tokenService.GetRefreshToken(httpContext);

                if (refreshToken == null)
                {
                    return ServiceResult<RefreshToken>.Failure("Refresh token is not valid", ServiceErrorType.ValidationError);
                }

                if (jWTClaim == null)
                {
                    return ServiceResult<RefreshToken>.Failure("Access token is not valid", ServiceErrorType.Unauthorized);
                }

                LogoutDTO logoutDTO = new LogoutDTO(jWTClaim.AccountID, refreshToken.ID);



                if (!(logoutDTO.idAccount == refreshToken.AccountID))
                {
                    return ServiceResult<RefreshToken>.Failure("This session does not belong to your account.", ServiceErrorType.Unauthorized);
                }

                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTimeOffset.UtcNow;

                await this.unitOfWork.Commit().ConfigureAwait(false);

                return ServiceResult<RefreshToken>.Success(refreshToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred during logout, error : {ex}");
                return ServiceResult<RefreshToken>.Failure("An unexpected internal error occurred during logout .", ServiceErrorType.InternalError);

            }
        }
    }
}
