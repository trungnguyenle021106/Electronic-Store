using APIGateway.Infrastructure.DTO.ContentManagement;
using APIGateway.Infrastructure.DTO.ContentManagement.Request;
using APIGateway.Infrastructure.DTO.Product;
using APIGateway.Infrastructure.Service;
using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using Sprache;

namespace APIGateway.Application.Usecases
{
    public class GWCreateUC
    {
        private readonly ProductService productService;
        private readonly ContentManagementService contentManagementService;
        private readonly ILogger<GWCreateUC> _logger;
        private readonly HandleServiceError handleServiceError;

        public GWCreateUC(ProductService productService, ContentManagementService contentManagementService, ILogger<GWCreateUC> logger,
            HandleServiceError handleServiceError)
        {
            this.productService = productService;
            this.contentManagementService = contentManagementService;
            this._logger = logger;
            this.handleServiceError = handleServiceError;
        }

        public async Task<ServiceResult<Filter>> CreateFilterAndFilterDetails(CreateFilterRequest createFilterRequest)
        {
            if (createFilterRequest == null)
            {
                return ServiceResult<Filter>.Failure("No filter provided to add.", ServiceErrorType.ValidationError);
            }

            if (createFilterRequest.Filter == null)
            {
                return ServiceResult<Filter>.Failure("Filter data cannot be null.", ServiceErrorType.ValidationError);
            }

            if (string.IsNullOrWhiteSpace(createFilterRequest.Filter.Position))
            {
                return ServiceResult<Filter>.Failure("Filter Position is required.", ServiceErrorType.ValidationError);
            }

            try
            {
                ServiceResult<ProductProperty> productResult = await this.productService.GetAllProductProperties();

                if (!productResult.IsSuccess)
                {

                    ErrorServiceResult errorResult = this.handleServiceError.
                        MapServiceError(productResult.ServiceErrorType ?? ServiceErrorType.InternalError, "Get ProductProperty");
                    return ServiceResult<Filter>.Failure(errorResult.Message, errorResult.ServiceErrorType);
                }

                List<int> matchedProductPropertyIds = productResult.ListItem
                                                        .Select(pp => pp.ID)
                                                        .Intersect(createFilterRequest.productPropertyIDs)
                                                        .ToList();

                if (!matchedProductPropertyIds.Any() && createFilterRequest.productPropertyIDs.Any())
                {
                    return ServiceResult<Filter>.Failure(
                        $"None of the provided product property IDs ({string.Join(", ", createFilterRequest.productPropertyIDs)}) match existing properties.",
                        ServiceErrorType.ValidationError);
                }
                else if (!createFilterRequest.productPropertyIDs.Any())
                {
                    _logger.LogInformation("No product property IDs provided in the request. Creating filter without specific product property associations.");
                }


                CreateFilterRequest requestForContentService =
                    new CreateFilterRequest(createFilterRequest.Filter, matchedProductPropertyIds);

                ServiceResult<Filter> createFilterResult = await this.contentManagementService.CreateFilterAndFilterDetails(requestForContentService);

                if (createFilterResult.IsSuccess)
                {
                    return ServiceResult<Filter>.Success(createFilterResult.ListItem);
                }
                else
                {
                    ErrorServiceResult errorResult = this.handleServiceError.
                      MapServiceError(createFilterResult.ServiceErrorType ?? ServiceErrorType.InternalError, "Create Filter");
                    return ServiceResult<Filter>.Failure(errorResult.Message, errorResult.ServiceErrorType);
                }
            }
            catch (Exception ex)
            {
                // 8. Bắt các lỗi không mong muốn
                _logger.LogError(ex, "An unexpected internal error occurred during filter and filter details creation.");

                return ServiceResult<Filter>.Failure(
                    "An unexpected internal error occurred during filter and filter details creation.",
                    ServiceErrorType.InternalError
                );
            }
        }

    }
}
