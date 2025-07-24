
using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Interface.UnitOfWork;
using ContentManagementService.Domain.Request;
using ContentManagementService.Infrastructure.DTO;
using ContentManagementService.Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ContentManagementService.Application.Usecases
{
    public class CreateContentManagementUC
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ProductService productService;
        private readonly HandleServiceError handleServiceError;

        public CreateContentManagementUC(IUnitOfWork unitOfWork, ProductService productService, HandleServiceError handleServiceError)
        {
            this.unitOfWork = unitOfWork;
            this.productService = productService;
            this.handleServiceError = handleServiceError;
        }

        public async Task<ServiceResult<Filter>> CreateFilterAndFilterDetails(CreateUpdateFilterRequest createFilterReques)
        {
            if (createFilterReques.Filter == null)
            {
                return ServiceResult<Filter>.Failure(
                    "Filter data cannot be null.",
                    ServiceErrorType.ValidationError);
            }
            if (string.IsNullOrWhiteSpace(createFilterReques.Filter.Position))
            {
                return ServiceResult<Filter>.Failure(
                "Filter Position is required.",
                ServiceErrorType.ValidationError);
            }

            if (createFilterReques.productPropertyIDs == null || !createFilterReques.productPropertyIDs.Any())
            {
                return ServiceResult<Filter>.Failure(
                    "At least one ProductProperty ID is required for a filter.",
                    ServiceErrorType.ValidationError);
            }
            try
            {
                Filter? existFilter = await this.unitOfWork.FilterRepository().
                 GetAll().
                 Where(item => item.Position == createFilterReques.Filter.Position).
                 FirstOrDefaultAsync();

                if (existFilter != null)
                {
                    return ServiceResult<Filter>.Failure(
                          "This position is already exist.",
                          ServiceErrorType.AlreadyExists);
                }

                ServiceResult<ProductProperty> productResult = await this.productService.GetAllProductProperties();
                if (!productResult.IsSuccess)
                {
                    ErrorServiceResult errorResult = this.handleServiceError.
                       MapServiceError(productResult.ServiceErrorType ?? ServiceErrorType.InternalError, "Get ProductProperty");
                    return ServiceResult<Filter>.Failure(errorResult.Message, errorResult.ServiceErrorType);
                }

                List<int> distinctProductPropertyIDs = createFilterReques.productPropertyIDs?.Distinct().ToList() ?? new List<int>();
                List<int> validProductPropertyIDs = productResult.ListItem
                    .Where(pp => distinctProductPropertyIDs.Contains(pp.ID))
                    .Select(pp => pp.ID)
                    .ToList();

                if (!validProductPropertyIDs.Any())
                {
                    return ServiceResult<Filter>.Failure(
                        "None of the provided ProductProperty IDs are valid.",
                        ServiceErrorType.ValidationError);
                }


                using (var transaction = await this.unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        await this.unitOfWork.FilterRepository().Add(createFilterReques.Filter);
                        await this.unitOfWork.Commit();


                        List<FilterDetail> filterDetailsToAdd = validProductPropertyIDs.Select(ppId => new FilterDetail
                        {
                            FilterID = createFilterReques.Filter.ID,
                            Filter = createFilterReques.Filter,
                            ProductPropertyID = ppId
                        }).ToList();

                        await this.unitOfWork.FilterDetailRepository().AddRangeAsync(filterDetailsToAdd);
                        await this.unitOfWork.Commit();
                        await this.unitOfWork.CommitTransactionAsync(transaction);

                        Filter newFilter = await this.unitOfWork.FilterRepository().GetById(createFilterReques.Filter.ID);

                        return ServiceResult<Filter>.Success(newFilter);
                    }
                    catch (Exception ex)
                    {
                        await this.unitOfWork.RollbackAsync(transaction);
                        Console.WriteLine($"Error starting transaction: {ex}");
                        return ServiceResult<Filter>.Failure(
                            "An unexpected error occurred while starting the transaction. Please try again.", ServiceErrorType.InternalError);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating filter and details: {ex}");

                return ServiceResult<Filter>.Failure(
                    "An unexpected error occurred while creating the filter and its details. Please try again.", ServiceErrorType.InternalError);
            }
        }
    }
}
