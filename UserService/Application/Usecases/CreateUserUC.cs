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
            // Bước 1: Kiểm tra tính hợp lệ của RefreshToken đầu vào
            int accountID = refreshToken?.AccountID ?? 0;
            if (accountID <= 0)
            {
                // Debug point 1: Đặt breakpoint ở đây. Nếu vào đây, nghĩa là RefreshToken đầu vào bị null hoặc không có AccountID hợp lệ.
                // --> Gợi ý: Có thể do cookie RefreshToken không được gửi từ client hoặc không được đọc đúng cách.
                return ServiceResult<string>.Failure("No refresh token provided to add.", ServiceErrorType.ValidationError);
            }

            if (refreshToken.IsRevoked == true || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                // Debug point 2: Đặt breakpoint ở đây. Nếu vào đây, nghĩa là RefreshToken đã hết hạn hoặc bị thu hồi.
                // --> Gợi ý: Kiểm tra xem RefreshToken trong DB có IsRevoked là true không, hoặc ExpiresAt có nhỏ hơn thời gian hiện tại không.
                return ServiceResult<string>.Failure("Refresh token is expired or revoked.", ServiceErrorType.Invalid);
            }

            try
            {
                // Bước 2: Lấy thông tin tài khoản và khách hàng
                int? customerID = this.unitOfWork.CustomerRepository()
                    .GetAll()
                    .Where(c => c.AccountID == accountID)
                    .Select(c => c.ID)
                    .FirstOrDefault();

                Account account = await unitOfWork.AccountRepository().GetById(accountID).ConfigureAwait(false);

                // Debug point 3: Kiểm tra 'account' và 'customerID' ở đây.
                // Nếu 'account' là null, hoặc 'customerID' là null khi đáng lẽ phải có, điều đó có thể là nguyên nhân.
                // Mặc dù GetById sẽ ném lỗi nếu không tìm thấy, nhưng hãy kiểm tra.

                // Bước 3: Tạo Access Token mới
                string accessToken = this.tokenService.GenerateAccessToken(new JWTClaim(account.ID, account.Role, customerID));

                // Debug point 4: Đặt breakpoint ở đây. KIỂM TRA GIÁ TRỊ CỦA `accessToken`.
                // -> Đây là điểm then chốt! Nếu `accessToken` ở đây là null hoặc rỗng, thì ServiceResult<string>.Success(accessToken) sẽ trả về một kết quả rỗng.
                // -> Từ đó, khi hàm SetTokenCookie được gọi với result.Item, nó sẽ nhận giá trị rỗng và ném lỗi `ArgumentException`.

                return ServiceResult<string>.Success(accessToken); // Chỉ trả về AccessToken!
            }
            catch (Exception ex)
            {
                // Debug point 5: Đặt breakpoint ở đây. Nếu vào đây, nghĩa là có lỗi trong quá trình tạo token hoặc truy vấn DB.
                // -> Xem `ex` để biết chi tiết lỗi.
                Console.Error.WriteLine($"Error creating JWT: {ex}");
                return ServiceResult<string>.Failure(
                    "An unexpected internal error occurred during JWT creation.",
                    ServiceErrorType.InternalError
                );
            }
        }
    }
}
