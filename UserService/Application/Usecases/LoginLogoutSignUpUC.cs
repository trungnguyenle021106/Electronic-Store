using ApiDto.Response;
using CommonDto.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Security.Principal;
using UserService.Application.Service;
using UserService.Domain.DTO;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Domain.Request;
using UserService.Domain.Response;
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

        public async Task<LoginResult<LoginSignUpDTO>> LoginAccount(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return LoginResult<LoginSignUpDTO>.Failure("Email và mật khẩu không được để trống.", LoginErrorType.ValidationError);
            }

            try
            {
                Account? account = await this.unitOfWork.AccountRepository()
                                                    .GetAll()
                                                    .FirstOrDefaultAsync(a => a.Email == email)
                                                    .ConfigureAwait(false);

                if (account == null)
                {
                    return LoginResult<LoginSignUpDTO>.Failure("Email hoặc mật khẩu không đúng.", LoginErrorType.NotFound);
                }

                if (!hashService.VerifyHash(password, account.Password))
                {
                    return LoginResult<LoginSignUpDTO>.Failure("Email hoặc mật khẩu không đúng.", LoginErrorType.InvalidCredentials);
                }


                //if (account.Role == "Admin")
                //{
                //    return LoginResult<LoginSignUpDTO>.Failure("Tài khoản của bạn không hợp lệ.", LoginErrorType.ValidationError);
                //}

                if (account.Status == "Locked")
                {
                    return LoginResult<LoginSignUpDTO>.Failure("Tài khoản của bạn bị khóa.", LoginErrorType.AccountLocked);
                }


                string accessToken = tokenService.GenerateAccessToken(new JWTClaim
                (
                    account.ID, account.Role
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
                return LoginResult<LoginSignUpDTO>.Success(new LoginSignUpDTO
                (
                    account, accessToken, refreshToken
                ));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred during login for email: {email}, error : {ex}");
                return LoginResult<LoginSignUpDTO>.Failure(
                    "An unexpected internal error occurred during login .",
                    LoginErrorType.InternalError
                );
            }
        }

        public async Task<CreationResult<LoginSignUpDTO>> SignUp(SignUpRequest signUpRequest)
        {
            try
            {
                if (signUpRequest == null)
                {
                    return CreationResult<LoginSignUpDTO>
                       .Failure("No sign up information provided to add.", CreationErrorType.ValidationError);
                }

             
                if (!(await emailValidator.CheckEmailValid(signUpRequest.Email).ConfigureAwait(false)).Status)
                {
                    return CreationResult<LoginSignUpDTO>
                     .Failure("Email is not valid.", CreationErrorType.ValidationError);
                }


                IQueryable<Account> queryAccount = this.unitOfWork.AccountRepository().GetAll();
                queryAccount = queryAccount.Where(a => a.Email == signUpRequest.Email);

                Account? account = await queryAccount.FirstOrDefaultAsync().ConfigureAwait(false);
                if (account != null)
                {
                    return CreationResult<LoginSignUpDTO>.Failure("Account is exist", CreationErrorType.AlreadyExists);
                }

                Account newAccount = new Account
                {
                    Email = signUpRequest.Email,
                    Password = hashService.Hash(signUpRequest.Password),
                    Role = "Customer",
                    Status = "Active"
                };
                await this.unitOfWork.AccountRepository().Add(newAccount).ConfigureAwait(false);


                IQueryable<Customer> queryCustomer = this.unitOfWork.CustomerRepository().GetAll();
                queryCustomer = queryCustomer.Where(a => a.AccountID == newAccount.ID);

                Customer? customer = await queryCustomer.FirstOrDefaultAsync().ConfigureAwait(false);
                if (customer != null)
                {
                    return CreationResult<LoginSignUpDTO>.Failure("Customer is exist", CreationErrorType.AlreadyExists);
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


                string accessToken = tokenService.GenerateAccessToken(new JWTClaim(newAccount.ID, newAccount.Role));

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


                return CreationResult<LoginSignUpDTO>.Success(new LoginSignUpDTO
                (
                   newAccount, accessToken, refreshToken
                ));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error sign up : {ex}");

                return CreationResult<LoginSignUpDTO>.Failure(
                    "An unexpected internal error occurred during sign up .",
                    CreationErrorType.InternalError
                );
            }
        }

        public async Task<UpdateResult<RefreshToken>> LogoutSpecificSession(HttpContext httpContext)
        {
            try
            {
                JWTClaim? jWTClaim = tokenService.GetJWTClaim(httpContext);
                RefreshToken? refreshToken = await tokenService.GetRefreshToken(httpContext);

                if (refreshToken == null)
                {
                    return UpdateResult<RefreshToken>.Failure("Refresh token is not valid", UpdateErrorType.ValidationError);
                }

                if (jWTClaim == null)
                {
                    return UpdateResult<RefreshToken>.Failure("Access token is not valid", UpdateErrorType.Unauthorized);
                }

                LogoutDTO logoutDTO = new LogoutDTO(jWTClaim.IDAccount, refreshToken.ID);



                if (!(logoutDTO.idAccount == refreshToken.AccountID))
                {
                    return UpdateResult<RefreshToken>.Failure("This session does not belong to your account.", UpdateErrorType.Unauthorized);
                }

                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTimeOffset.UtcNow;

                await this.unitOfWork.Commit().ConfigureAwait(false);

                return UpdateResult<RefreshToken>.Success(refreshToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred during logout, error : {ex}");
                return UpdateResult<RefreshToken>.Failure("An unexpected internal error occurred during logout .", UpdateErrorType.InternalError);

            }
        }
    }
}
