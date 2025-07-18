using CommonDto.ResultDTO;
using System.Net;

namespace CommonDto.HandleErrorResult
{
    public class ErrorServiceResult
    {
        public string Message { get; set; }
        public ServiceErrorType ServiceErrorType { get; set; }
        public ErrorServiceResult(string message, ServiceErrorType serviceErrorType)
        {
            Message = message;
            ServiceErrorType = serviceErrorType;
        }
    }

    public class HandleServiceError
    {
        public ServiceErrorType MapStatusCodeToServiceErrorType(System.Net.HttpStatusCode statusCode, string? errorContent = null)
        {
            switch (statusCode)
            {
                case System.Net.HttpStatusCode.Conflict:
                    return ServiceErrorType.AlreadyExists;
                case System.Net.HttpStatusCode.BadRequest:
                    // Có thể kiểm tra errorContent nếu bạn có một cấu trúc ProblemDetails
                    // từ dịch vụ bên ngoài để phân biệt ValidationError và Invalid
                    // Ví dụ: if (errorContent?.Contains("ValidationErrors") == true)
                    return ServiceErrorType.ValidationError; // hoặc ServiceErrorType.Invalid
                case System.Net.HttpStatusCode.UnprocessableEntity: // 422
                    // Thường dùng cho lỗi ngữ nghĩa khi yêu cầu hợp lệ về mặt cú pháp nhưng không xử lý được
                    return ServiceErrorType.ValidationError; // Rất phù hợp cho ValidationError
                case System.Net.HttpStatusCode.NotFound: // 404
                    return ServiceErrorType.NotFound;
                case System.Net.HttpStatusCode.Unauthorized: // 401
                    return ServiceErrorType.Unauthorized;
                case System.Net.HttpStatusCode.Forbidden: // 403
                    // Có thể phân biệt giữa AccountLocked và Unauthorized khác (ví dụ, thiếu quyền)
                    // nếu errorContent cung cấp đủ thông tin.
                    return ServiceErrorType.AccountLocked; // Hoặc Unauthorized tùy ngữ cảnh
                case System.Net.HttpStatusCode.TooManyRequests: // 429
                    // Có thể thêm một ServiceErrorType mới cho RateLimitExceeded
                    return ServiceErrorType.InternalError; // Hoặc một loại lỗi cụ thể hơn nếu có
                case System.Net.HttpStatusCode.BadGateway: // 502
                case System.Net.HttpStatusCode.ServiceUnavailable: // 503
                case System.Net.HttpStatusCode.GatewayTimeout: // 504
                    return ServiceErrorType.InternalError; // Lỗi liên quan đến hạ tầng hoặc dịch vụ không khả dụng

                // Thêm các trường hợp khác nếu cần thiết (ví dụ: 405 Method Not Allowed, v.v.)
                default:
                    // Đối với bất kỳ lỗi 4xx nào khác không được xử lý rõ ràng, có thể là Invalid
                    if ((int)statusCode >= 400 && (int)statusCode < 500)
                    {
                        return ServiceErrorType.Invalid; // Lỗi do client gửi request sai định dạng/nghiệp vụ
                    }
                    // Đối với bất kỳ lỗi 5xx nào khác
                    return ServiceErrorType.InternalError; // Mặc định là lỗi nội bộ
            }
        }


        public ErrorServiceResult MapServiceError(ServiceErrorType serviceErrorType, string operation)
        {           
            string errorMessage = "";
            ServiceErrorType errorType = ServiceErrorType.InternalError;
            switch (serviceErrorType)
            {
                case ServiceErrorType.Unauthorized:
                    errorMessage = errorMessage ?? $"Unauthorized access during {operation}.";
                    errorType = ServiceErrorType.Unauthorized; // Gán loại lỗi cụ thể
                    break;
                case ServiceErrorType.InvalidCredentials:
                    errorMessage = errorMessage ?? $"Invalid credentials provided during {operation}.";
                    errorType = ServiceErrorType.InvalidCredentials;
                    break;
                case ServiceErrorType.RepositoryTypeMismatch:
                    errorMessage = errorMessage ?? $"Repository type mismatch during {operation}.";
                    errorType = ServiceErrorType.RepositoryTypeMismatch; // Gán loại lỗi cụ thể
                    break;
                case ServiceErrorType.Invalid:
                   errorMessage = errorMessage ?? $"Invalid data provided during {operation}.";
                    errorType = ServiceErrorType.Invalid; // Gán loại lỗi cụ thể
                    break;
                case ServiceErrorType.NotFound:
                    errorMessage = errorMessage ?? $"Item not found during {operation}.";
                    errorType = ServiceErrorType.NotFound; // Gán loại lỗi cụ thể
                    break;
                case ServiceErrorType.ValidationError:
                    errorMessage = errorMessage ?? $"Validation error occurred during {operation}.";
                    errorType = ServiceErrorType.ValidationError; // Gán loại lỗi cụ thể
                    break;
                case ServiceErrorType.InternalError:
                    errorMessage = errorMessage ?? $"An internal error occurred during {operation}.";
                    errorType = ServiceErrorType.InternalError; // Gán loại lỗi cụ thể
                    break;
                case ServiceErrorType.AlreadyExists:
                    errorMessage = errorMessage ?? $"An item with similar properties already exists during {operation}.";
                    errorType = ServiceErrorType.AlreadyExists; // Gán loại lỗi cụ thể
                    break;
                // Thêm các trường hợp lỗi khác nếu có
                default:
                    errorMessage = errorMessage ?? $"An unexpected error occurred during {operation}.";
                    errorType = ServiceErrorType.InternalError; // Gán loại lỗi mặc định nếu không xác định
                    break;
            }

            return new ErrorServiceResult(errorMessage, errorType); // Trả về ErrorResult với thông điệp và loại lỗi

        }
        
       

    }
}
