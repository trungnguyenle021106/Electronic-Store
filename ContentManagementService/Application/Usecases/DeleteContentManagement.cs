
using CommonDto.ResultDTO;
using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Interface.UnitOfWork;

namespace ContentManagementService.Application.Usecases
{
    public class DeleteContentManagement
    {
        private readonly IUnitOfWork unitOfWork;
        public DeleteContentManagement(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<ServiceResult<Filter>> DeleteFilter(int id)
        {
            if (id <= 0)
            {
                return ServiceResult<Filter>.Failure("Filter ID is invalid.", ServiceErrorType.ValidationError);
            }
            try
            {
                Filter? filter = await this.unitOfWork.FilterRepository().GetById(id);
                if (filter == null)
                {
                    return ServiceResult<Filter>.Failure($"Filter with ID : '{id}' is not exist.",
                        ServiceErrorType.NotFound);
                }
                this.unitOfWork.FilterRepository().Delete(filter);
                await this.unitOfWork.Commit();
                return ServiceResult<Filter>.Success(filter);
            }
            catch (Exception ex)
            {
                return ServiceResult<Filter>.Failure($"An error occurred while deleting the filter: {ex.Message}",
                    ServiceErrorType.InternalError);
            }
        }
    }
}
