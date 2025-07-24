using CommonDto.ResultDTO;
using ContentManagementService.Application.Usecases;
using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Request;
using ContentManagementService.Infrastructure.Data.DBContext;
using ContentManagementService.Infrastructure.DTO;
using Microsoft.AspNetCore.Mvc;

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
            }).RequireAuthorization("OnlyAdmin");
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
                ServiceResult<PagedResult<Filter>> result = await getContentManagementUC.GetPagedFilters(page,pageSize,searchText,filter);
                return handleResultApi.MapServiceResultToHttp(result);
            });
        }

        public static void GetAllProductPropertiesOfFilter(this WebApplication app)
        {
            app.MapGet("/filters/{filterID}/product-properties", async (HttpContext httpContext, GetContentManagementUC getContentManagementUC, HandleResultApi handleResultApi,
                int filterID) =>
            {
                ServiceResult<ProductProperty> result = await getContentManagementUC.GetAllProductPropertiesOfFilter(filterID);
                return handleResultApi.MapServiceResultToHttp(result);
            });
        }

        public static void GetFilterById(this WebApplication app)
        {
            app.MapGet("/filters/{id}", async (HttpContext httpContext, GetContentManagementUC getContentManagementUC, HandleResultApi handleResultApi,
                int id) =>
            {
                ServiceResult<Filter> result = await getContentManagementUC.GetFilterByID(id);
                return handleResultApi.MapServiceResultToHttp(result);
            });
        }
        #endregion

        #region Update Filter USECASE
        public static void MapUpdateFilterUseCaseAPIs
            (this WebApplication app)
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
            }).RequireAuthorization("OnlyAdmin");
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
            }).RequireAuthorization("OnlyAdmin");
        }
        #endregion
    }
}
