
using APIGateway.Infrastructure.DTO.ContentManagement;
using APIGateway.Infrastructure.DTO.Product;
using APIGateway.Infrastructure.Service;
using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;

namespace APIGateway.Application.Usecases
{
    public class GWGetUC
    {
        private readonly ProductService productService;
        private readonly ContentManagementService contentManagementService;
        private readonly ILogger<GWCreateUC> _logger;
        private readonly HandleServiceError handleServiceError;

        public GWGetUC(ProductService productService, ContentManagementService contentManagementService, ILogger<GWCreateUC> logger,
            HandleServiceError handleServiceError)
        {
            this.productService = productService;
            this.contentManagementService = contentManagementService;
            this._logger = logger;
            this.handleServiceError = handleServiceError;
        }

        public async Task<ServiceResult<ProductProperty>> getAllPropertiesOfFilter(int filterID)
        {
            if (filterID <= 0)
            {
                return ServiceResult<ProductProperty>.Failure("Invalid filter ID provided.", ServiceErrorType.ValidationError);
            }
            try
            {
                ServiceResult<FilterDetail> filterDetailsResult = await this.contentManagementService.GetAllFilterDetailByFilterID(filterID);
                if (!filterDetailsResult.IsSuccess)
                {
                    ErrorServiceResult errorResult = new HandleServiceError().MapServiceError(
                        filterDetailsResult.ServiceErrorType ?? ServiceErrorType.InternalError, "Get Filter Details");
                    return ServiceResult<ProductProperty>.Failure(errorResult.Message, errorResult.ServiceErrorType);
                }
                List<int> productPropertyIDs = filterDetailsResult.ListItem.Select(fd => fd.ProductPropertyID).ToList();

                ServiceResult<ProductProperty> productPropertiesResult = await this.productService.GetAllProductProperties();
                if (!productPropertiesResult.IsSuccess)
                {
                    ErrorServiceResult errorResult = new HandleServiceError().MapServiceError(
                        productPropertiesResult.ServiceErrorType ?? ServiceErrorType.InternalError, "Get Product Properties");
                    return ServiceResult<ProductProperty>.Failure(errorResult.Message, errorResult.ServiceErrorType);
                }
                List<ProductProperty> matchedProductProperties = productPropertiesResult.ListItem
                    .Where(pp => productPropertyIDs.Contains(pp.ID))
                    .ToList();
                return ServiceResult<ProductProperty>.Success(matchedProductProperties);
            }
            catch (Exception ex)
            {
                // 8. Bắt các lỗi không mong muốn
                _logger.LogError(ex, "An unexpected internal error occurred while getting properties of filter.");
                return ServiceResult<ProductProperty>.Failure(
                    "An unexpected internal error occurred while getting properties of filter.",
                    ServiceErrorType.InternalError
                );
            }


        }
    }
}
