using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using ContentManagementService.Infrastructure.DTO;

namespace ContentManagementService.Infrastructure.Service
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger;
        private readonly HandleServiceError handleServiceError;


        public ProductService(HttpClient httpClient, ILogger<ProductService> logger, HandleServiceError handleServiceError)
        {
            this._httpClient = httpClient;
            this._logger = logger;
            this.handleServiceError = handleServiceError;
        }

        public async Task<ServiceResult<ProductProperty>> GetAllProductProperties()
        {
            using var response = await _httpClient.GetAsync("productProperties");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var results = await response.Content.ReadFromJsonAsync<List<ProductProperty>>();
                    if (results == null)
                    {
                        _logger.LogWarning("Received successful response from Product Service, but content was empty or null.");
                        return ServiceResult<ProductProperty>.Success(new List<ProductProperty>());
                    }
                    return ServiceResult<ProductProperty>.Success(results);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize Product Service response.");
                    return ServiceResult<ProductProperty>.Failure("Failed to process service response.", ServiceErrorType.InternalError);
                }
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                string errorMessage = $"Error calling ProductPropertyService: HTTP {response.StatusCode} - {errorContent}";
                _logger.LogError(errorMessage);

                ServiceErrorType ServiceErrorType = this.handleServiceError.MapStatusCodeToServiceErrorType(response.StatusCode, errorContent);
                return ServiceResult<ProductProperty>.Failure(errorMessage, ServiceErrorType);
            }
        }
    }
}
