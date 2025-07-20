using APIGateway.Infrastructure.DTO.ContentManagement;
using APIGateway.Infrastructure.DTO.ContentManagement.Request;
using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using Sprache;

namespace APIGateway.Infrastructure.Service
{
    public class ContentManagementService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger;
        private readonly HandleServiceError handleServiceError;

        public ContentManagementService(HttpClient httpClient, ILogger<ProductService> logger, HandleServiceError handleServiceError)
        {
            this._httpClient = httpClient;
            this._logger = logger;
            this.handleServiceError = handleServiceError;
        }


        public async Task<ServiceResult<Filter>> CreateFilterAndFilterDetails(CreateFilterRequest createFilterRequest)
        {

             var response = await _httpClient.PostAsJsonAsync("/filters", createFilterRequest);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // Sử dụng ProductPropertyDto để ánh xạ từ phản hồi của Product Service
                    var result = await response.Content.ReadFromJsonAsync<Filter>();

                    if (result == null)
                    {
                        Console.WriteLine("Received successful response from ContentManagement Service, but content was empty or null.");
                        return ServiceResult<Filter>.Success(result); // Trả về danh sách rỗng nếu nội dung null
                    }
                    return ServiceResult<Filter>.Success(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to deserialize ContentManagement Service response : {ex}");
                    return ServiceResult<Filter>.Failure("Failed to process service response.", ServiceErrorType.InternalError);
                }
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                string errorMessage = $"Error calling ContentManagement Service: HTTP {response.StatusCode} - {errorContent}";
                Console.WriteLine(errorMessage); // Ghi log lỗi

                // Phân loại lỗi dựa trên mã trạng thái HTTP

                ServiceErrorType ServiceErrorType = this.handleServiceError.MapStatusCodeToServiceErrorType(response.StatusCode, errorContent);
                return ServiceResult<Filter>.Failure(errorMessage, ServiceErrorType);
            }
        }

        public async Task<ServiceResult<FilterDetail>> GetAllFilterDetailByFilterID(int filterID)
        {
            using var response = await _httpClient.GetAsync($"/filters/{filterID}/details");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var results = await response.Content.ReadFromJsonAsync<List<FilterDetail>>();
                    if (results == null)
                    {
                        _logger.LogWarning("Received successful response from Product Service, but content was empty or null.");
                        return ServiceResult<FilterDetail>.Success(new List<FilterDetail>());
                    }
                    return ServiceResult<FilterDetail>.Success(results);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize Product Service response.");
                    return ServiceResult<FilterDetail>.Failure("Failed to process service response.", ServiceErrorType.InternalError);
                }
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                string errorMessage = $"Error calling ProductPropertyService: HTTP {response.StatusCode} - {errorContent}";
                _logger.LogError(errorMessage);

                ServiceErrorType ServiceErrorType = this.handleServiceError.MapStatusCodeToServiceErrorType(response.StatusCode, errorContent);
                return ServiceResult<FilterDetail>.Failure(errorMessage, ServiceErrorType);
            }
        }
    }
}
