using UserService.Domain.DTO;
using Verifalia.Api;
using Verifalia.Api.EmailValidations.Models;

namespace UserService.Infrastructure.Verify_Email
{
    public class EmailValidatorService  // Triển khai interface
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailValidatorService> _logger;
        private readonly VerifaliaRestClient _verifalia; // Sử dụng _verifalia instance

        public EmailValidatorService(IConfiguration configuration, ILogger<EmailValidatorService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Đọc API Keys từ cấu hình
            string? verifaliaUsername = _configuration["Verifalia:Username"];
            string? verifaliaPassword = _configuration["Verifalia:Password"];

            if (string.IsNullOrEmpty(verifaliaUsername) || string.IsNullOrEmpty(verifaliaPassword))
            {
                _logger.LogError("Verifalia API keys are not configured in appsettings.json. Please set 'Verifalia:Username' and 'Verifalia:Password'.");
                // Tùy chọn: ném ngoại lệ nếu cấu hình không đầy đủ để ngăn ứng dụng khởi động
                throw new InvalidOperationException("Verifalia API keys are missing in configuration.");
            }

            // Khởi tạo VerifaliaRestClient với API keys từ cấu hình
            _verifalia = new VerifaliaRestClient(verifaliaUsername, verifaliaPassword);
        }

        public async Task<EmailValidatorDTO> CheckEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Validation request received with empty or null email.");
                return new EmailValidatorDTO(false, "Email không được để trống.");
            }

            try
            {
                var job = await _verifalia.EmailValidations.SubmitAsync(email);
                var entry = job.Entries[0];

                // --- SỬA ĐỔI LẠI ĐỂ KHẮC PHỤC LỖI TẠM THỜI ---
                // Chỉ dựa vào Classification và Status nếu không tìm thấy Errors/Description/Warnings
                string validationDetail = entry.Status.ToString(); // Lấy tên trạng thái làm thông tin chi tiết

                // Nếu Verifalia có cung cấp thêm thuộc tính như "Cause", "Reason", v.v., bạn có thể thêm vào đây
                // Ví dụ: if (entry.SomeOtherDetailProperty != null) validationDetail = entry.SomeOtherDetailProperty;

                _logger.LogInformation("Email '{Email}' validation result: Classification={Classification}, Status={Status}, Detail={Detail}",
                                       email, entry.Classification, entry.Status, validationDetail);
                // --- KẾT THÚC SỬA ĐỔI ---

                if (entry.Classification == ValidationEntryClassification.Deliverable)
                {
                    return new EmailValidatorDTO(true, "Email hợp lệ.");
                }
                else
                {
                    // Trả về email không hợp lệ và chi tiết từ Status
                    return new EmailValidatorDTO(false, $"Email không hợp lệ: {validationDetail}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi xác thực email '{Email}'.", email);
                return new EmailValidatorDTO(false, "Đã xảy ra lỗi trong quá trình xác thực email. Vui lòng thử lại sau.");
            }
        }
    }
}
