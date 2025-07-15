using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDto.ResultDTO
{
    public class ServiceResult<T>
    {
        public T? Item { get; private set; } // Renamed for generic clarity
        public List<T> ListItem { get; private set; }
        public string? ErrorMessage { get; private set; }
        public ServiceErrorType? ServiceErrorType { get; private set; } // Use the generic error type

        public bool IsSuccess => ServiceErrorType == null;

        private ServiceResult() { } // Prevents direct instantiation

        // The static Success method now accepts T
        public static ServiceResult<T> Success(T createdItem)
        {
            return new ServiceResult<T> { Item = createdItem };
        }

        public static ServiceResult<T> Success(List<T> createdItems)
        {
            return new ServiceResult<T> { ListItem = createdItems };
        }

        // The static Failure method remains the same, but it uses the generic error type
        public static ServiceResult<T> Failure(string errorMessage, ServiceErrorType errorType)
        {
            return new ServiceResult<T> { ErrorMessage = errorMessage, ServiceErrorType = errorType };
        }

        public static ServiceResult<T> NoContent() 
        {
            return new ServiceResult<T> { Item = default(T), ListItem = null };
        }
    }


    public enum ServiceErrorType
    {
        AlreadyExists,       
        RepositoryTypeMismatch,
        InternalError,
        ValidationError,
        Invalid,
        NotFound, 
        InvalidCredentials,
        AccountLocked,
        Unauthorized
    }
}
