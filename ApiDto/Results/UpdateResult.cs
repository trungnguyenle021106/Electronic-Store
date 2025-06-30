using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDto.Response
{
    public class UpdateResult<T> where T : class
    {
        // The type of the successfully updated item
        public T? UpdatedItem { get; private set; }
        public string? ErrorMessage { get; private set; }
        public UpdateErrorType? ErrorType { get; private set; }

        public bool IsSuccess => ErrorType == null;

        private UpdateResult() { } // Prevents direct instantiation

        // Static method for successful update
        public static UpdateResult<T> Success(T updatedItem)
        {
            return new UpdateResult<T> { UpdatedItem = updatedItem };
        }

        // Static method for failed update
        public static UpdateResult<T> Failure(string errorMessage, UpdateErrorType errorType)
        {
            return new UpdateResult<T> { ErrorMessage = errorMessage, ErrorType = errorType };
        }
    }

    public enum UpdateErrorType
    {
        NotFound,             
        ValidationError,
        InternalError,
        Unauthorized
    }
}
