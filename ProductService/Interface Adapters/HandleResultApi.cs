using CommonDto.ResultDTO;
using Microsoft.AspNetCore.Mvc;

namespace ProductService.Interface_Adapters
{
    public class HandleResultApi
    {
        public IResult MapServiceResultToHttp<T>(ServiceResult<T> serviceResult)
        {
            if (serviceResult.IsSuccess)
            {
                // Logic xử lý thành công
                if (serviceResult.Item != null)
                {
                    return Results.Ok(serviceResult.Item);
                }
                if (serviceResult.ListItem != null && serviceResult.ListItem.Any())
                {
                    return Results.Ok(serviceResult.ListItem);
                }
                return Results.NoContent(); // Thành công nhưng không có nội dung trả về
            }

            // Xử lý các loại lỗi khác nhau dựa trên ServiceErrorType
            return serviceResult.ServiceErrorType switch
            {
                ServiceErrorType.AlreadyExists => Results.Conflict(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Title = "Resource Already Exists",
                        Detail = serviceResult.ErrorMessage
                    }
                ),
                ServiceErrorType.ValidationError => Results.BadRequest(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation Error",
                        Detail = serviceResult.ErrorMessage
                    }
                ),
                ServiceErrorType.NotFound => Results.NotFound(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Not Found",
                        Detail = serviceResult.ErrorMessage
                    }
                ),
                ServiceErrorType.Unauthorized => Results.Unauthorized(), // 401 Unauthorized
                ServiceErrorType.InvalidCredentials => Results.Unauthorized(), // 401 Unauthorized (credentials specifically)
                ServiceErrorType.RepositoryTypeMismatch => Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Server Configuration Error",
                    detail: serviceResult.ErrorMessage ?? "A repository type mismatch occurred, indicating a server configuration problem."
                ),
                ServiceErrorType.InternalError => Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal Server Error",
                    detail: serviceResult.ErrorMessage ?? "An unexpected internal error occurred."
                ),
                ServiceErrorType.Invalid => Results.BadRequest(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Invalid Input Data",
                        Detail = serviceResult.ErrorMessage ?? "The provided data is invalid."
                    }
                ),
                ServiceErrorType.AccountLocked => Results.Forbid(), // 403 Forbidden (người dùng bị cấm truy cập tài nguyên)  
                _ => Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "An Unexpected Error Occurred",
                    detail: serviceResult.ErrorMessage ?? "An unknown error occurred. Please try again later."
                )
            };
        }
    }
}
