
using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using ContentManagementService.Application.UnitOfWork;
using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Interface.UnitOfWork;
using ContentManagementService.Domain.Request;
using ContentManagementService.Infrastructure.Data.DBContext;
using ContentManagementService.Infrastructure.DTO;
using ContentManagementService.Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ContentManagementService.Application.Usecases
{
    public class UpdateContentManagementUC
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ProductService productService;
        private readonly HandleServiceError handleServiceError;
        public UpdateContentManagementUC(IUnitOfWork unitOfWork, ProductService productService, HandleServiceError handleServiceError)
        {
            this.unitOfWork = unitOfWork;
            this.productService = productService;
            this.handleServiceError = handleServiceError;
        }

        public async Task<ServiceResult<Filter>> UpdateFilterAndFilterDetails(CreateUpdateFilterRequest updateFilterRequest)
        {
            if (updateFilterRequest.Filter == null)
            {
                return ServiceResult<Filter>.Failure(
                    "Filter data cannot be null.",
                    ServiceErrorType.ValidationError);
            }
            if (string.IsNullOrWhiteSpace(updateFilterRequest.Filter.Position))
            {
                return ServiceResult<Filter>.Failure(
                "Filter Position is required.",
                ServiceErrorType.ValidationError);
            }

            if (updateFilterRequest.productPropertyIDs == null || !updateFilterRequest.productPropertyIDs.Any())
            {
                return ServiceResult<Filter>.Failure(
                    "At least one ProductProperty ID is required for a filter.",
                    ServiceErrorType.ValidationError);
            }

            try
            {
                Filter? existFilter = await this.unitOfWork.FilterRepository().
                GetAll().
                Where(item => item.Position == updateFilterRequest.Filter.Position).
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

                List<FilterDetail> filterDetails = this.unitOfWork.FilterDetailRepository().GetAll()
                    .Where(fd => fd.FilterID == updateFilterRequest.Filter.ID)
                    .ToList();

                List<FilterDetail> filterDetailsToAdd = productResult.ListItem.
                Where(pp => !filterDetails.Any(fd => fd.ProductPropertyID == pp.ID)).
                Select(pp => new FilterDetail
                {
                    FilterID = updateFilterRequest.Filter.ID,
                    ProductPropertyID = pp.ID
                }).
                ToList();

                List<FilterDetail> filterDetailsToRemove = filterDetails
                    .Where(fd => !updateFilterRequest.productPropertyIDs.Contains(fd.ProductPropertyID))
                    .ToList();

                using (var transaction = await this.unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        existFilter.Position = updateFilterRequest.Filter.Position;


                        if (filterDetailsToAdd.Any())
                        {
                            await this.unitOfWork.FilterDetailRepository().AddRangeAsync(filterDetailsToAdd);
                        }

                        if (filterDetailsToRemove.Any())
                        {
                            await this.unitOfWork.FilterDetailRepository().RemoveRange(filterDetailsToRemove);
                        }

                        await this.unitOfWork.CommitTransactionAsync(transaction);
                        return ServiceResult<Filter>.Success(existFilter);
                    }
                    catch (Exception ex)
                    {
                        await this.unitOfWork.RollbackAsync(transaction);
                        return ServiceResult<Filter>.Failure(
                            $"An unexpected error occurred while starting the transaction: {ex.Message}",
                            ServiceErrorType.InternalError);
                    }
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<Filter>.Failure(
                    $"An unexpected error occurred while updating the filter: {ex.Message}",
                    ServiceErrorType.InternalError);
            }
        }
    }
}
