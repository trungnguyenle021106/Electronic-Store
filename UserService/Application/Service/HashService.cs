using BCrypt.Net;

namespace UserService.Application.Service
{
    public class HashService
    {
        public string Hash(string stringNeedToHash)
        {
            // BCrypt.Net.BCrypt.HashPassword sẽ tạo ra một salt ngẫu nhiên và hash chuỗi token.
            // Kết quả trả về đã bao gồm salt, nên khi xác minh không cần truyền salt riêng.
            // Work factor (độ phức tạp) mặc định là 12, thường là đủ tốt.
            return BCrypt.Net.BCrypt.HashPassword(stringNeedToHash);
        }

        public bool VerifyHash(string stringNeedToHash, string hashedString)
        {
            // BCrypt.Net.BCrypt.Verify sẽ lấy salt từ hashedToken, hash lại token
            // với salt đó, và so sánh kết quả.
            return BCrypt.Net.BCrypt.Verify(stringNeedToHash, hashedString);
        }
    }
}
