using UserService.Domain.DTO;
using Verifalia.Api;
using Verifalia.Api.EmailValidations.Models;

namespace UserService.Infrastructure.Verify_Email
{
    public class EmailValidator
    {
        public async Task<EmailValidatorDTO> CheckEmailValid(string email)
        {
            try
            {
                // Khởi tạo VerifaliaRestClient với API keys của bạn
                var verifalia = new VerifaliaRestClient("f4edf41478a24f04ac5e1ceaa1fb7163", "0937210476Nn");

                // Gửi yêu cầu xác thực email
                var job = await verifalia.EmailValidations.SubmitAsync(email);

                // Lấy kết quả xác thực cho email đầu tiên trong job
                // (Giả định bạn chỉ gửi một email mỗi lần)
                var entry = job.Entries[0];

                // Kiểm tra phân loại kết quả từ Verifalia
                if (entry.Classification == ValidationEntryClassification.Deliverable)
                {
                    return new EmailValidatorDTO(true, "Email hợp lệ");
                }

                // Trả về email không hợp lệ nếu phân loại không phải là Deliverable
                return new EmailValidatorDTO(false, "Email không hợp lệ");
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi để tiện gỡ lỗi và theo dõi
                Console.Error.WriteLine($"Lỗi khi kiểm tra email '{email}': {ex.Message}");
                // Có thể ghi thêm ex.StackTrace để biết vị trí lỗi chi tiết hơn
                // Console.Error.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Trả về EmailValidatorDTO báo hiệu lỗi và thông báo
                // Bạn có thể tùy chỉnh thông báo lỗi ở đây
                return new EmailValidatorDTO(false, $"Đã xảy ra lỗi trong quá trình xác thực email: {ex.Message}");
            }
        }
    }
}
