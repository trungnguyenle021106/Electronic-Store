using ApiDto.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using UserService.Application.Service;
using UserService.Application.UnitOfWorks;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Infrastructure.Data.DBContext;

namespace UserService.Application.Usecases
{
    public class UpdateUserUC
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly HashService hashService;

        public UpdateUserUC(IUnitOfWork unitOfWork, HashService hashService)
        {
            this.unitOfWork = unitOfWork;
            this.hashService = hashService;
        }

        public async Task<UpdateResult<Customer>> UpdateCustomerInformation(int customerID, Customer newCustomer)
        {
            try
            {
                if (customerID <= 0)
                {
                    return UpdateResult<Customer>.Failure("Customer ID is invalid.", UpdateErrorType.ValidationError);
                }

                if (newCustomer == null)
                {
                    return UpdateResult<Customer>.Failure("No Customer provided to update.", UpdateErrorType.ValidationError);
                }


                Customer? customer = await this.unitOfWork.CustomerRepository().GetById(customerID).ConfigureAwait(false);

                if (customer == null)
                {
                    return UpdateResult<Customer>.Failure("Customer not found.", UpdateErrorType.NotFound);
                }

                customer.Phone = newCustomer.Phone;
                customer.Name = newCustomer.Name;
                customer.Gender = newCustomer.Gender;
                customer.Address = newCustomer.Address;

                await unitOfWork.Commit().ConfigureAwait(false);

                return UpdateResult<Customer>.Success(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Customer: " + ex.ToString());
                return UpdateResult<Customer>.Failure("An internal error occurred during Customer update.", UpdateErrorType.InternalError);
            }
        }

        public async Task<UpdateResult<Account>> UpdateAccountPassword(int accountID, string oldPassword, string newPassword)
        {
            try
            {
                if (accountID <= 0)
                {
                    return UpdateResult<Account>.Failure("Account ID is invalid.", UpdateErrorType.ValidationError);
                }

                if (newPassword == null || oldPassword == null)
                {
                    return UpdateResult<Account>.Failure("No password provided to update.", UpdateErrorType.ValidationError);
                }

                Account? account = await this.unitOfWork.AccountRepository().GetById(accountID).ConfigureAwait(false);
                if (account == null)
                {
                    return UpdateResult<Account>.Failure("Account not found.", UpdateErrorType.NotFound);
                }

                if (!(hashService.VerifyHash(oldPassword, account.Password)))
                {
                    return UpdateResult<Account>.Failure("Old password is not true.", UpdateErrorType.ValidationError);
                }

                account.Password = newPassword;

                await unitOfWork.Commit().ConfigureAwait(false);

                return UpdateResult<Account>.Success(account);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật mật khẩu Account: " + ex.ToString());
                return UpdateResult<Account>.Failure("An internal error occurred during Account update password.", UpdateErrorType.InternalError);
            }
        }

        public async Task<UpdateResult<Account>> UpdateAccountStatus(int accountID, string status)
        {
            try
            {
                if (accountID <= 0)
                {
                    return UpdateResult<Account>.Failure("Account ID is invalid.", UpdateErrorType.ValidationError);
                }

                if (status == null)
                {
                    return UpdateResult<Account>.Failure("No status provided to update.", UpdateErrorType.ValidationError);
                }

                Account? account = await this.unitOfWork.AccountRepository().GetById(accountID).ConfigureAwait(false);
                if (account == null)
                {
                    return UpdateResult<Account>.Failure("Account not found.", UpdateErrorType.NotFound);
                }

                account.Status = status;

                await unitOfWork.Commit().ConfigureAwait(false);

                return UpdateResult<Account>.Success(account);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Account: " + ex.ToString());
                return UpdateResult<Account>.Failure("An internal error occurred during Account update.", UpdateErrorType.InternalError);
            }
        }
     

        public async Task<UpdateResult<RefreshToken>> RevokeRefreshToken(int refreshTokenID)
        {
            try
            {
                if (refreshTokenID <= 0)
                {
                    return UpdateResult<RefreshToken>.Failure("Refresh token ID is invalid.", UpdateErrorType.ValidationError);
                }

                RefreshToken? refreshToken = await this.unitOfWork.RefreshTokenRepository().GetById(refreshTokenID).ConfigureAwait(false);
                if (refreshToken == null)
                {
                    return UpdateResult<RefreshToken>.Failure($"Refresh token with ID : {refreshTokenID} not found", UpdateErrorType.NotFound);
                }

                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTimeOffset.UtcNow;

                await unitOfWork.Commit().ConfigureAwait(false);

                return UpdateResult<RefreshToken>.Success(refreshToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred during revoke refresh token, error : {ex}");
                return UpdateResult<RefreshToken>.Failure("An unexpected internal error occurred during revoke refresh token .", UpdateErrorType.InternalError);
            }
        }
    }
}
