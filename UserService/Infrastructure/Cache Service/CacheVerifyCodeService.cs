using Microsoft.Extensions.Caching.Memory;

namespace UserService.Infrastructure.Cache_Service
{
    public class CacheVerifyCodeService
    {
        private readonly IMemoryCache _memoryCache; // Thay đổi từ MemoryCache sang IMemoryCache

        // Constructor nhận IMemoryCache từ DI
        private readonly ILogger<CacheVerifyCodeService> _logger; // Thêm logger

        public CacheVerifyCodeService(IMemoryCache memoryCache, ILogger<CacheVerifyCodeService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public string GenerateVerifyCode(string email)
        {
            // Sử dụng Random.Shared cho tính ngẫu nhiên tốt hơn trong .NET 6+
            string code = Random.Shared.Next(100000, 999999).ToString();
            string cacheKey = $"OTP_{email}"; // Định nghĩa rõ ràng cache key

            try
            {
                // Thời gian hết hạn của OTP (ví dụ: 1 phút)
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

                _memoryCache.Set(cacheKey, code, cacheEntryOptions);
                _logger.LogInformation("Generated and cached OTP {OTPCode} for email {Email}.", code, email);
                return code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate or cache OTP for email {Email}.", email);
                // Bạn có thể chọn ném lại lỗi hoặc trả về một giá trị đặc biệt để báo hiệu thất bại
                throw; // Ném lại để controller biết có lỗi
            }
        }
        public bool VerifyCode(string code, string email)
        {
            string cacheKey = $"OTP_{email}";

            try
            {
                if (!_memoryCache.TryGetValue(cacheKey, out string? cachedCode))
                {
                    _logger.LogWarning("Xác thực thất bại cho email {Email}: Mã OTP không tìm thấy trong cache hoặc đã hết hạn.", email);
                    return false; // Không tìm thấy mã hoặc đã hết hạn
                }

                if (cachedCode != code)
                {
                    _logger.LogWarning("Xác thực thất bại cho email {Email}: Mã OTP không khớp. Mong đợi {ExpectedCode}, nhận được {ReceivedCode}.", email, cachedCode, code);
                    return false; // Mã không khớp
                }

                // Nếu khớp, xóa mã khỏi cache để không thể sử dụng lại
                _memoryCache.Remove(cacheKey);
                _logger.LogInformation("Mã OTP đã được xác thực thành công cho email {Email}.", email);
                return true;
            }
            catch (Exception ex)
            {
                // Ghi lại bất kỳ lỗi nào xảy ra trong quá trình xác thực cache
                _logger.LogError(ex, "Có lỗi xảy ra trong quá trình xác thực OTP cho email {Email}.", email);
                // Trả về false vì xác thực không thể hoàn tất hoặc có vấn đề nghiêm trọng
                return false;
            }
        }
    }
}
