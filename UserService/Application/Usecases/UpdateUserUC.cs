using CommonDto.ResultDTO;
using UserService.Application.Service;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;

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

        public async Task<ServiceResult<Customer>> UpdateCustomerInformation(int customerID, Customer newCustomer)
        {
            try
            {
                if (customerID <= 0)
                {
                    return ServiceResult<Customer>.Failure("Customer ID is invalid.", ServiceErrorType.ValidationError);
                }

                if (newCustomer == null)
                {
                    return ServiceResult<Customer>.Failure("No Customer provided to update.", ServiceErrorType.ValidationError);
                }

                Console.WriteLine($"Cập nhật thông tin Customer có ID: {customerID}");
                Customer? customer = await this.unitOfWork.CustomerRepository().GetById(customerID).ConfigureAwait(false);
                Console.WriteLine(customer.ID);
                if (customer == null)
                {
                    return ServiceResult<Customer>.Failure("Customer not found.", ServiceErrorType.NotFound);
                }

                customer.Phone = newCustomer.Phone;
                customer.Name = newCustomer.Name;
                customer.Gender = newCustomer.Gender;
                customer.Address = newCustomer.Address;

                await unitOfWork.Commit().ConfigureAwait(false);

                return ServiceResult<Customer>.Success(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Customer: " + ex.ToString());
                return ServiceResult<Customer>.Failure("An internal error occurred during Customer update.", ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Account>> UpdatePassword(string email, string newPassword)
        {
            if (email == null)
            {
                return ServiceResult<Account>.Failure("email is invalid", ServiceErrorType.ValidationError);
            }

            if (newPassword == null)
            {
                return ServiceResult<Account>.Failure("password is invalid", ServiceErrorType.ValidationError);
            }

            try
            {
                Account? account = this.unitOfWork.AccountRepository().GetAll().Where(a => a.Email == email).FirstOrDefault();
                if (account == null)
                {
                    return ServiceResult<Account>.Failure("Account not found.", ServiceErrorType.NotFound);
                }

                account.Password = hashService.Hash(newPassword);

                await unitOfWork.Commit().ConfigureAwait(false);

                return ServiceResult<Account>.Success(account);

            }
            catch (Exception ex) {
                Console.WriteLine("Lỗi cập nhật mật khẩu Account: " + ex.ToString());
                return ServiceResult<Account>.Failure("An internal error occurred during Account update password.", ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Account>> UpdateAccountPassword(int accountID, string oldPassword, string newPassword)
        {
            if (accountID <= 0)
            {
                return ServiceResult<Account>.Failure("Account ID is invalid.", ServiceErrorType.ValidationError);
            }

            if (newPassword == null || oldPassword == null)
            {
                return ServiceResult<Account>.Failure("No password provided to update.", ServiceErrorType.ValidationError);
            }
            try
            {
                Account? account = await this.unitOfWork.AccountRepository().GetById(accountID).ConfigureAwait(false);
                if (account == null)
                {
                    return ServiceResult<Account>.Failure("Account not found.", ServiceErrorType.NotFound);
                }

                if (!(hashService.VerifyHash(oldPassword, account.Password)))
                {
                    return ServiceResult<Account>.Failure("Old password is not true.", ServiceErrorType.ValidationError);
                }

                account.Password =  hashService.Hash(newPassword);

                await unitOfWork.Commit().ConfigureAwait(false);

                return ServiceResult<Account>.Success(account);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật mật khẩu Account: " + ex.ToString());
                return ServiceResult<Account>.Failure("An internal error occurred during Account update password.", ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Account>> UpdateAccountStatus(int accountID, string status)
        {
            try
            {
                if (accountID <= 0)
                {
                    return ServiceResult<Account>.Failure("Account ID is invalid.", ServiceErrorType.ValidationError);
                }

                if (status == null)
                {
                    return ServiceResult<Account>.Failure("No status provided to update.", ServiceErrorType.ValidationError);
                }

                Account? account = await this.unitOfWork.AccountRepository().GetById(accountID).ConfigureAwait(false);
                if (account == null)
                {
                    return ServiceResult<Account>.Failure("Account not found.", ServiceErrorType.NotFound);
                }

                account.Status = status;

                await unitOfWork.Commit().ConfigureAwait(false);

                return ServiceResult<Account>.Success(account);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Account: " + ex.ToString());
                return ServiceResult<Account>.Failure("An internal error occurred during Account update.", ServiceErrorType.InternalError);
            }
        }


        public async Task<ServiceResult<RefreshToken>> RevokeRefreshToken(int refreshTokenID)
        {
            try
            {
                if (refreshTokenID <= 0)
                {
                    return ServiceResult<RefreshToken>.Failure("Refresh token ID is invalid.", ServiceErrorType.ValidationError);
                }

                RefreshToken? refreshToken = await this.unitOfWork.RefreshTokenRepository().GetById(refreshTokenID).ConfigureAwait(false);
                if (refreshToken == null)
                {
                    return ServiceResult<RefreshToken>.Failure($"Refresh token with ID : {refreshTokenID} not found", ServiceErrorType.NotFound);
                }

                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTimeOffset.UtcNow;

                await unitOfWork.Commit().ConfigureAwait(false);

                return ServiceResult<RefreshToken>.Success(refreshToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred during revoke refresh token, error : {ex}");
                return ServiceResult<RefreshToken>.Failure("An unexpected internal error occurred during revoke refresh token .", ServiceErrorType.InternalError);
            }
        }
    }
}
