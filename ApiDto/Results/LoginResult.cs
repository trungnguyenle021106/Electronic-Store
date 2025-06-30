using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDto.Results
{
    public class LoginResult<T>
    {
        public bool IsSuccess => ErrorType == null;
        public T? Value { get; private set; }
        public string? ErrorMessage { get; }
        public LoginErrorType? ErrorType { get; }

        // Private constructor cho trạng thái thành công
        private LoginResult(T value)
        {
            Value = value;
        }

        private LoginResult(string errorMessage, LoginErrorType errorType)
        {
            ErrorMessage = errorMessage;
            ErrorType = errorType;
        }

        public static LoginResult<T> Success(T value)
        {
            return new LoginResult<T>(value);
        }

        public static LoginResult<T> Failure(string errorMessage, LoginErrorType errorType)
        {
            return new LoginResult<T>(errorMessage, errorType);
        }
    }

    public enum LoginErrorType
    {
        ValidationError,      // Dữ liệu đầu vào không hợp lệ (ví dụ: email/password trống)
        InvalidCredentials,   // Email hoặc mật khẩu không đúng
        NotFound,             // Tài khoản không tồn tại (thường gộp vào InvalidCredentials để bảo mật)
        AccountLocked,        // Tài khoản bị khóa
        InternalError         // Lỗi nội bộ không mong muốn
    }
}
