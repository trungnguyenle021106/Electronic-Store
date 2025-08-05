using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using AnalyticService.Infrastructure.DTO;

namespace AnalyticService.Infrastructure.Service
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

        public async Task<ServiceResult<Product>> CheckExistProducts(List<int> productIDs)
        {
            if (productIDs == null || !productIDs.Any())
            {
                _logger.LogWarning("Product IDs cannot be null or empty.");
                return ServiceResult<Product>.Failure("Product IDs cannot be null or empty.", ServiceErrorType.ValidationError);
            }
            try
            {
                using var response = await _httpClient.PostAsJsonAsync("/products/exist", productIDs);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var results = await response.Content.ReadFromJsonAsync<List<Product>>();
                        if (results == null)
                        {
                            _logger.LogWarning("Received successful response from Product Service, but content was empty or null.");
                            return ServiceResult<Product>.Success(new List<Product>());
                        }

                        return ServiceResult<Product>.Success(results);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize Product Service response.");
                        return ServiceResult<Product>.Failure("Failed to process service response.", ServiceErrorType.InternalError);
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    string errorMessage = $"Error calling ProductPropertyService: HTTP {response.StatusCode} - {errorContent}";
                    _logger.LogError(errorMessage);

                    ServiceErrorType ServiceErrorType = this.handleServiceError.MapStatusCodeToServiceErrorType(response.StatusCode, errorContent);
                    return ServiceResult<Product>.Failure(errorMessage, ServiceErrorType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call Product Service.");
                return ServiceResult<Product>.Failure("Failed to call Product Service.", ServiceErrorType.InternalError);
            }
        } 
    }
}
