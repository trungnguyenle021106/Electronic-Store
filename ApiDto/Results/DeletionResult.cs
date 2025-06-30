using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDto.Response
{
    public class DeletionResult<T> where T : class
    {
        // The type of the successfully deleted item will be T
        public T? DeletedItem { get; private set; }
        public List<T>? DeletedItems { get; private set; }
        public string? ErrorMessage { get; private set; }
        public DeletionErrorType? ErrorType { get; private set; }

        public bool IsSuccess => ErrorType == null;

        private DeletionResult() { } // Prevents direct instantiation

        // Static method for a successful single deletion
        public static DeletionResult<T> Success(T deletedItem)
        {
            return new DeletionResult<T> { DeletedItem = deletedItem };
        }

        // Static method for a successful deletion with no specific item returned (e.g., just confirmation)
        public static DeletionResult<T> Success(List<T> deletedItems)
        {
            return new DeletionResult<T> {DeletedItems = deletedItems };
        }

        // Static method for a failed deletion
        public static DeletionResult<T> Failure(string errorMessage, DeletionErrorType errorType)
        {
            return new DeletionResult<T> { ErrorMessage = errorMessage, ErrorType = errorType };
        }
    }

    // Enum for common deletion error types
    public enum DeletionErrorType
    {
        NotFound,             // Item to be deleted was not found
        InternalError,        // Generic internal server error
        ValidationError,      // Input validation failed for the delete request
    }
}
