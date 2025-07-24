using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using ContentManagementService.Application.UnitOfWork;
using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Interface.UnitOfWork;
using ContentManagementService.Infrastructure.Data.DBContext;
using ContentManagementService.Infrastructure.DTO;
using ContentManagementService.Infrastructure.Service;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ContentManagementService.Application.Usecases
{
    public class GetContentManagementUC
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ProductService productService;
        private readonly HandleServiceError handleServiceError;

        public GetContentManagementUC(IUnitOfWork unitOfWork, ProductService productService, HandleServiceError handleServiceError)
        {
            this.unitOfWork = unitOfWork;
            this.productService = productService;
            this.handleServiceError = handleServiceError;
        }

        public async Task<ServiceResult<ProductProperty>> GetAllProductPropertiesOfFilter(int id)
        {

            if (id <= 0)
            {
                return ServiceResult<ProductProperty>.Failure("Product ID is invalid.", ServiceErrorType.ValidationError);
            }
            try
            {
                IQueryable<FilterDetail> query = this.unitOfWork.FilterDetailRepository().GetAll();
                query = query. Where(fd => fd.FilterID == id);

                List<FilterDetail>? listFilterDetai = await query.ToListAsync();

                if (listFilterDetai == null)
                {
                    return ServiceResult<ProductProperty>.Failure($"Product with ID : '{id}' is not exist.",
                        ServiceErrorType.NotFound);
                }

                ServiceResult<ProductProperty> productResult = await this.productService.GetAllProductProperties();
                if (!productResult.IsSuccess)
                {
                    ErrorServiceResult errorResult = this.handleServiceError.
                       MapServiceError(productResult.ServiceErrorType ?? ServiceErrorType.InternalError, "Get ProductProperty");
                    return ServiceResult<ProductProperty>.Failure(errorResult.Message, errorResult.ServiceErrorType);
                }

                List<ProductProperty> productProperties = productResult.ListItem.Join(
                    listFilterDetai,
                    pp => pp.ID,
                    fd => fd.ProductPropertyID,
                    (pp, fd) => pp
                ).ToList();

                return ServiceResult<ProductProperty>.Success(productProperties);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy sản phẩm có id : {id}, lỗi : {ex.Message}");
                return ServiceResult<ProductProperty>.Failure("An unexpected internal error occurred while GetAllPropertiesOfProduct.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<PagedResult<Filter>>> GetPagedFilters(
     int? page,
     int? pageSize,
     string? searchText, // Dùng string? cho phép null
     string? filter)    // Dùng string? cho phép null
        {
            try
            {
                IQueryable<Filter> query = this.unitOfWork.FilterRepository().GetAll();

                // 1. Áp dụng tìm kiếm (Search) vào tất cả các cột có thể
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string searchLower = searchText.ToLower();

                    // Điều chỉnh các cột của ProductProperty mà bạn muốn tìm kiếm
                    query = query.Where(pp =>
                        (pp.Position != null && pp.Position.ToLower().Contains(searchLower))
                    );
                }

                // 2. Áp dụng lọc (Filter) theo tên thuộc tính (ProductProperty.Name)
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string filterLower = filter.ToLower();
                    // Lọc trực tiếp trên cột Name của ProductProperty
                    query = query.Where(pp => pp.Position != null && pp.Position.ToLower().Contains(filterLower));
                }

                // 3. Lấy tổng số lượng bản ghi sau khi áp dụng tất cả các điều kiện lọc và tìm kiếm
                int totalCount = await query.CountAsync();

                List<Filter> list = new List<Filter>();
                // 4. Kiểm tra trang hợp lệ
                if (page.HasValue && pageSize.HasValue)
                {
                    if (page < 1)
                    {
                        page = 1;
                    }
                    query = query.OrderBy(pp => pp.ID); // Sắp xếp mặc định theo ID

                    // 6. Áp dụng phân trang (Skip và Take)
                     list = await query
                        .Skip((int)((page - 1) * pageSize))
                        .Take((int)pageSize)
                        .ToListAsync();
                }
                else
                {
                    list = await query.ToListAsync();
                    page = 1;
                    pageSize = 1; 
                }
               
                return ServiceResult<PagedResult<Filter>>.Success(new PagedResult<Filter>
                {
                    Items = list,
                    Page = (int)page,
                    PageSize = (int)pageSize,
                    TotalCount = totalCount // Sử dụng totalCount đã được lọc/tìm kiếm
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy thuộc tính sản phẩm, lỗi : {ex.Message}");
                return ServiceResult<PagedResult<Filter>>.Failure("An unexpected internal error occurred while GetPagedProductProperties.",
                                    ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Filter>> GetFilterByID(int filterID)
        {
            try
            {
                Filter? filter = await this.unitOfWork.FilterRepository().GetById(filterID);
                if (filter == null) {
                    return ServiceResult<Filter>.Failure($"Filter with ID : '{filterID}' is not exist.",
                        ServiceErrorType.NotFound);
                }
               return ServiceResult<Filter>.Success(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy filter id : {filterID} lỗi : {ex}");
                return ServiceResult<Filter>.Failure("An unexpected internal error occurred while GetFilterByID.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<ProductProperty>> GetProductPropertiesOfFilterByPosition(string position)
        {
            try
            {
                IQueryable<Filter> queryFilter = this.unitOfWork.FilterRepository().GetAll();
                Filter? filter = await queryFilter.Where(f => f.Position == position).FirstOrDefaultAsync();
                if (filter == null) { 
                return ServiceResult<ProductProperty>.Failure($"Filter with position : '{position}' is not exist.",
                        ServiceErrorType.NotFound);
                }

                int id = filter.ID;

                IQueryable<FilterDetail> query = this.unitOfWork.FilterDetailRepository().GetAll();
                query = query.Where(fd => fd.FilterID == id);

                List<FilterDetail>? listFilterDetai = await query.ToListAsync();

                if (listFilterDetai == null)
                {
                    return ServiceResult<ProductProperty>.Failure($"Product with ID : '{id}' is not exist.",
                        ServiceErrorType.NotFound);
                }

                ServiceResult<ProductProperty> productResult = await this.productService.GetAllProductProperties();
                if (!productResult.IsSuccess)
                {
                    ErrorServiceResult errorResult = this.handleServiceError.
                       MapServiceError(productResult.ServiceErrorType ?? ServiceErrorType.InternalError, "Get ProductProperty");
                    return ServiceResult<ProductProperty>.Failure(errorResult.Message, errorResult.ServiceErrorType);
                }

                List<ProductProperty> productProperties = productResult.ListItem.Join(
                    listFilterDetai,
                    pp => pp.ID,
                    fd => fd.ProductPropertyID,
                    (pp, fd) => pp
                ).ToList();

                return ServiceResult<ProductProperty>.Success(productProperties);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy filter tại position : {position} lỗi : {ex}");
                return ServiceResult<ProductProperty>.Failure("An unexpected internal error occurred while GetProductPropertiesOfFilterByPosition.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<List<Filter>?> GetAllFilter()
        {
            try
            {
                IQueryable<Filter> query = this.unitOfWork.FilterRepository().GetAll();
                List<Filter> list = await query.ToListAsync();
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách filter : {ex}");
                return null;
            }
        }
    }
}
