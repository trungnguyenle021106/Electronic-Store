
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
using System.Linq;

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
            // --- 1. Xác thực đầu vào (Validation) ---
            // Đảm bảo Filter object không rỗng
            if (updateFilterRequest.Filter == null)
            {
                return ServiceResult<Filter>.Failure(
                    "Filter data cannot be null.",
                    ServiceErrorType.ValidationError);
            }

            // Đảm bảo Position không rỗng hoặc chỉ chứa khoảng trắng
            if (string.IsNullOrWhiteSpace(updateFilterRequest.Filter.Position))
            {
                return ServiceResult<Filter>.Failure(
                    "Filter Position is required.",
                    ServiceErrorType.ValidationError);
            }

            // Đảm bảo danh sách ProductPropertyIDs không rỗng hoặc null
            if (updateFilterRequest.productPropertyIDs == null || !updateFilterRequest.productPropertyIDs.Any())
            {
                return ServiceResult<Filter>.Failure(
                    "At least one ProductProperty ID is required for a filter.",
                    ServiceErrorType.ValidationError);
            }

            try
            {
                Filter? existFilter = await this.unitOfWork.FilterRepository()
                    .GetAll()
                    .Where(item => item.Position == updateFilterRequest.Filter.Position)
                    .FirstOrDefaultAsync();

                // Nếu không tìm thấy filter, trả về lỗi Not Found
                if (existFilter == null)
                {
                    return ServiceResult<Filter>.Failure(
                        "Filter with the specified position does not exist.",
                        ServiceErrorType.NotFound);
                }

                ServiceResult<ProductProperty> productResult = await this.productService.GetAllProductProperties();
                if (!productResult.IsSuccess)
                {

                    ErrorServiceResult errorResult = this.handleServiceError.
                        MapServiceError(productResult.ServiceErrorType ?? ServiceErrorType.InternalError, "Get ProductProperty");
                    return ServiceResult<Filter>.Failure(errorResult.Message, errorResult.ServiceErrorType);
                }


                HashSet<int> validProductPropertyIdsInSystem = new HashSet<int>(productResult.ListItem.Select(pp => pp.ID));

                if (updateFilterRequest.productPropertyIDs.Any(id => !validProductPropertyIdsInSystem.Contains(id)))
                {
                    return ServiceResult<Filter>.Failure(
                        "One or more provided ProductProperty IDs are invalid or do not exist in the system.",
                        ServiceErrorType.ValidationError);
                }

                List<FilterDetail> currentFilterDetails = await this.unitOfWork.FilterDetailRepository().GetAll()
                    .Where(fd => fd.FilterID == existFilter.ID)
                    .ToListAsync(); // Sử dụng ToListAsync() cho truy vấn bất đồng bộ


                HashSet<int> currentProductPropertyIdsInDetails = new HashSet<int>(currentFilterDetails.Select(fd => fd.ProductPropertyID));


                List<FilterDetail> filterDetailsToAdd = updateFilterRequest.productPropertyIDs
                    .Where(newPpId => !currentProductPropertyIdsInDetails.Contains(newPpId))
                    .Select(newPpId => new FilterDetail
                    {
                        FilterID = existFilter.ID, // Gán với ID của filter đã tồn tại
                        ProductPropertyID = newPpId
                    })
                    .ToList();

          
                List<FilterDetail> filterDetailsToRemove = currentFilterDetails
                    .Where(fd => !updateFilterRequest.productPropertyIDs.Contains(fd.ProductPropertyID))
                    .ToList();

                // --- 6. Thực hiện giao dịch Database để đảm bảo tính nhất quán ---
                using (var transaction = await this.unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        existFilter.Position = updateFilterRequest.Filter.Position;
                        this.unitOfWork.FilterRepository().Update(existFilter); // Đảm bảo Repository có phương thức Update hoặc Entity Framework đang theo dõi thay đổi.

                        // Thêm các FilterDetail mới nếu có
                        if (filterDetailsToAdd.Any())
                        {
                            await this.unitOfWork.FilterDetailRepository().AddRangeAsync(filterDetailsToAdd);
                        }

                        // Xóa các FilterDetail không còn cần thiết nếu có
                        if (filterDetailsToRemove.Any())
                        {
                            this.unitOfWork.FilterDetailRepository().RemoveRange(filterDetailsToRemove);
                        }

                        // Lưu tất cả các thay đổi vào database trong một lần commit của UnitOfWork
                        await this.unitOfWork.Commit();

                        // Commit giao dịch database
                        await this.unitOfWork.CommitTransactionAsync(transaction);

                        // Trả về kết quả thành công với Filter đã được cập nhật
                        return ServiceResult<Filter>.Success(existFilter);
                    }
                    catch (Exception ex)
                    {
                        // Nếu có lỗi trong giao dịch, thực hiện rollback
                        await this.unitOfWork.RollbackAsync(transaction);
                        return ServiceResult<Filter>.Failure(
                            $"An error occurred during the filter update transaction: {ex.Message}", // Thông báo lỗi rõ ràng
                            ServiceErrorType.InternalError);
                    }
                }
            }
            catch (Exception ex)
            {
                // Bắt các lỗi xảy ra bên ngoài khối giao dịch (ví dụ: lỗi xác thực ban đầu, lỗi khi lấy dữ liệu từ ProductService)
                return ServiceResult<Filter>.Failure(
                    $"An unexpected error occurred while processing the filter update: {ex.Message}",
                    ServiceErrorType.InternalError);
            }
        }
    }
}
