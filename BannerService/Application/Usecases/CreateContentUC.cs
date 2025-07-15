using CommonDto.ResultDTO;
using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Interface.UnitOfWork;
using ContentManagementService.Domain.Request;
using Microsoft.EntityFrameworkCore;

namespace ContentManagementService.Application.Usecases
{
    public class CreateContentUC
    {
        private readonly IUnitOfWork unitOfWork;

        public CreateContentUC(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<Filter>> CreateFilterAndFilterDetails(CreateFilterRequest createFilterReques)
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

                await this.unitOfWork.FilterRepository().Add(createFilterReques.Filter);

                List<int> distinctProductPropertyIDs = createFilterReques.productPropertyIDs?.Distinct().ToList() ?? new List<int>();
                List<FilterDetail> filterDetailsToAdd = new List<FilterDetail>();
                foreach (int ppId in distinctProductPropertyIDs)
                {
                    FilterDetail newFilterDetail = new FilterDetail
                    {
                        FilterID = createFilterReques.Filter.ID,
                        Filter = createFilterReques.Filter,
                        ProductPropertyID = ppId
                    };
                    filterDetailsToAdd.Add(newFilterDetail);
                }

                await this.unitOfWork.FilterDetailRepository().AddRangeAsync(filterDetailsToAdd);

                await this.unitOfWork.Commit();

                Filter newFilter = await this.unitOfWork.FilterRepository().GetById(createFilterReques.Filter.ID);

                return ServiceResult<Filter>.Success(newFilter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating filter and details: {ex}");

                return ServiceResult<Filter>.Failure(
                    "An unexpected error occurred while creating the filter and its details. Please try again.",
                    ServiceErrorType.InternalError);
            }
        }

        public async Task<FilterDetail?> CreateFilterDetail(FilterDetail filterDetail)
        {
            try
            {
                FilterDetail newFilterDetail = await this.unitOfWork.FilterDetailRepository().Add(filterDetail);
                await this.unitOfWork.Commit();
                return newFilterDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi tạo filter detail : " + ex.ToString());
                return null;
            }
        }
    }
}
