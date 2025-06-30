using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDto.Response
{
    public class QueryResult<T> where T : class
    {
        public T? Item { get; private set; }
        public List<T> Items { get; private set; } = new List<T>();
        public string? ErrorMessage { get; private set; }
        public RetrievalErrorType? ErrorType { get; private set; }

        public bool IsSuccess => ErrorType == null;

        private QueryResult() { }

      
        public static QueryResult<T> Success(T item)
        {         
            if (item == null)
            {        
                return new QueryResult<T> { Item = null };
            }
            return new QueryResult<T> { Item = item };
        }

        public static QueryResult<T> Success(List<T> items)
        {
            return new QueryResult<T> { Items = items ?? new List<T>() };
        }

        public static QueryResult<T> Failure(string errorMessage, RetrievalErrorType errorType)
        {
            return new QueryResult<T> { ErrorMessage = errorMessage, ErrorType = errorType };
        }
    }

  
    public enum RetrievalErrorType
    {
        NotFound,           
        InternalError,      
        ValidationError  
    }
}
