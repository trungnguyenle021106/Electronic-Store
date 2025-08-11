using CommonDto.ResultDTO;
using ContentManagementService.Application.Usecases;
using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Request;
using ContentManagementService.Infrastructure.Data.DBContext;
using ContentManagementService.Infrastructure.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace ContentManagementService.Interface_Adapters.APIs
{
    public static class ContentManagementAPI
    {
        public static void MapContentManagementEndpoints(this WebApplication app)
        {
            MapCreateContentManagementUseCaseAPIs(app);
            MapGetFilterUsecaseAPIs(app);
            MapUpdateFilterUseCaseAPIs(app);
            MapDeleteFilterUseCaseAPIs(app);
        }

        #region Create Filter USECASE
        public static void MapCreateContentManagementUseCaseAPIs(this WebApplication app)
        {
            CreateFilterAndFilterDetails(app);
        }

        public static void CreateFilterAndFilterDetails(this WebApplication app)
        {
            app.MapPost("/filters", async (HttpContext httpContext, CreateContentManagementUC createContentUC, HandleResultApi handleResultApi,
                [FromBody] CreateUpdateFilterRequest createFilterRequest) =>
            {
                ServiceResult<Filter> result = await createContentUC.CreateFilterAndFilterDetails(createFilterRequest);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Tạo Filter và Filter Details";
                operation.Description = "Tạo một bộ lọc và các chi tiết bộ lọc liên quan.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Filters" } };
                return operation;
            });
        }
        #endregion

        #region Get Filter USECASE
        public static void MapGetFilterUsecaseAPIs(this WebApplication app)
        {
            GetPagedFilter(app);
            GetAllProductPropertiesOfFilter(app);
            GetFilterById(app);
        }

        public static void GetPagedFilter(this WebApplication app)
        {
            app.MapGet("/filters", async (HttpContext httpContext, GetContentManagementUC getContentManagementUC, HandleResultApi handleResultApi,
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromQuery] string? searchText,
                [FromQuery] string? filter) =>
            {
                ServiceResult<PagedResult<Filter>> result = await getContentManagementUC.GetPagedFilters(page, pageSize, searchText, filter);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách Filters theo trang";
                operation.Description = "Trả về danh sách bộ lọc được phân trang. Có thể tìm kiếm và lọc theo các tiêu chí khác.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Filters" } };
                operation.Parameters[0].Description = "Số trang cần lấy.";
                operation.Parameters[1].Description = "Số lượng mục trên mỗi trang.";
                operation.Parameters[2].Description = "Văn bản tìm kiếm (nếu có).";
                operation.Parameters[3].Description = "Bộ lọc tùy chỉnh (nếu có).";
                return operation;
            });
        }

        public static void GetAllProductPropertiesOfFilter(this WebApplication app)
        {
            app.MapGet("/filters/{filterID}/product-properties", async (HttpContext httpContext, GetContentManagementUC getContentManagementUC, HandleResultApi handleResultApi,
                int filterID) =>
            {
                ServiceResult<ProductProperty> result = await getContentManagementUC.GetAllProductPropertiesOfFilter(filterID);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy tất cả Product Properties của một Filter";
                operation.Description = "Lấy danh sách các thuộc tính sản phẩm liên quan đến một bộ lọc cụ thể.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Filters" } };
                operation.Parameters[0].Description = "ID của bộ lọc.";
                return operation;
            });
        }

        public static void GetFilterById(this WebApplication app)
        {
            app.MapGet("/filters/{id}", async (HttpContext httpContext, GetContentManagementUC getContentManagementUC, HandleResultApi handleResultApi,
                int id) =>
            {
                ServiceResult<Filter> result = await getContentManagementUC.GetFilterByID(id);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy Filter theo ID";
                operation.Description = "Lấy thông tin chi tiết của một bộ lọc dựa trên ID.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Filters" } };
                operation.Parameters[0].Description = "ID của bộ lọc cần tìm.";
                return operation;
            });
        }
        #endregion

        #region Update Filter USECASE
        public static void MapUpdateFilterUseCaseAPIs(this WebApplication app)
        {
            MapUpdateFilter(app);
        }

        public static void MapUpdateFilter(this WebApplication app)
        {
            app.MapPut("/filters/{id}", async (HttpContext httpContext, UpdateContentManagementUC updateContentManagementUC, HandleResultApi handleResultApi,
                [FromBody] CreateUpdateFilterRequest updateFilterRequest) =>
            {
                ServiceResult<Filter> result = await updateContentManagementUC.UpdateFilterAndFilterDetails(updateFilterRequest);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Cập nhật Filter và Filter Details";
                operation.Description = "Cập nhật thông tin của một bộ lọc và các chi tiết của nó.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Filters" } };
                return operation;
            });
        }
        #endregion

        #region Delete Filter USECASE
        public static void MapDeleteFilterUseCaseAPIs(this WebApplication app)
        {
            DeleteFilter(app);
        }

        public static void DeleteFilter(this WebApplication app)
        {
            app.MapDelete("/filters/{id}", async (HttpContext httpContext, DeleteContentManagement deleteContentManagement, HandleResultApi handleResultApi,
                int id) =>
            {
                ServiceResult<Filter> result = await deleteContentManagement.DeleteFilter(id);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Xóa Filter theo ID";
                operation.Description = "Xóa một bộ lọc dựa trên ID.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Filters" } };
                operation.Parameters[0].Description = "ID của bộ lọc cần xóa.";
                return operation;
            });
        }
        #endregion
    }
}
