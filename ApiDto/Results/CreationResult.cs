namespace ApiDto.Response
{
    public class CreationResult<T> where T : class
    {
        // The type of the successfully created item will now be T
        public T? CreatedItem { get; private set; } // Renamed for generic clarity
        public List<T> CreatedItems { get; private set; }
        public string? ErrorMessage { get; private set; }
        public CreationErrorType? ErrorType { get; private set; } // Use the generic error type

        public bool IsSuccess => ErrorType == null;

        private CreationResult() { } // Prevents direct instantiation

        // The static Success method now accepts T
        public static CreationResult<T> Success(T createdItem)
        {
            return new CreationResult<T> { CreatedItem = createdItem };
        }

        public static CreationResult<T> Success(List<T> createdItems)
        {
            return new CreationResult<T> { CreatedItems = createdItems };
        }

        // The static Failure method remains the same, but it uses the generic error type
        public static CreationResult<T> Failure(string errorMessage, CreationErrorType errorType)
        {
            return new CreationResult<T> { ErrorMessage = errorMessage, ErrorType = errorType };
        }
    }

    // In a common/shared location, e.g., YourApp.Domain/Common/Results/CreationErrorType.cs
    // This enum is generic enough to be used across different creation operations
    public enum CreationErrorType
    {
        AlreadyExists,       // More generic than ProductAlreadyExists
        RepositoryTypeMismatch,
        InternalError,
        ValidationError,
        Invalid
    }
}
