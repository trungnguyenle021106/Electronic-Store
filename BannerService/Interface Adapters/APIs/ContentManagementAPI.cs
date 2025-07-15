using CommonDto.ResultDTO;
using ContentManagementService.Application.Usecases;
using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Request;
using ContentManagementService.Infrastructure.Data.DBContext;
using Microsoft.AspNetCore.Mvc;

namespace ContentManagementService.Interface_Adapters.APIs
{
    public static class ContentManagementAPI
    {
        public static void MapContentManagementEndpoints(this WebApplication app)
        {
            MapCreateContentManagementUseCaseAPIs(app);
            MapGetBannerUsecaseAPIs(app);
            MapUpdateBannerUseCaseAPIs(app);
        }

        #region Create Banner USECASE
        public static void MapCreateContentManagementUseCaseAPIs(this WebApplication app)
        {
            CreateFilterAndFilterDetails(app);
        }

        public static void CreateFilterAndFilterDetails(this WebApplication app)
        {
            app.MapPost("/filters", async (HttpContext httpContext, CreateContentUC createContentUC, HandleResultApi handleResultApi,
                [FromBody] CreateFilterRequest createFilterRequest) =>
            {
                ServiceResult<Filter> result = await createContentUC.CreateFilterAndFilterDetails(createFilterRequest);

              return  handleResultApi.MappingErrorHttp(result);            
            }).RequireAuthorization("OnlyAdmin");
        }
        #endregion

        #region Get Banner USECASE
        public static void MapGetBannerUsecaseAPIs(this WebApplication app)
        {
            MapGetBannerByID(app);
            MapGetAllBanners(app);
        }

        public static void MapGetBannerByID(this WebApplication app)
        {
            //app.MapGet("/banners/{bannerId}", async (ContentManagementContext bannerContext, int bannerId) =>
            //{
            //    try
            //    {
            //        return Results.Ok(await new GetFilterUC(bannerContext).GetFilterByID(bannerId));
            //    }
            //    catch (Exception ex)
            //    {
            //        return Results.BadRequest(ex.Message);
            //    }
            //}).RequireAuthorization("OnlyAdmin");
        }

        public static void MapGetBannerByPosition(this WebApplication app)
        {
            //app.MapGet("/banners/{bannerId}", async (ContentManagementContext bannerContext, string position) =>
            //{
            //    try
            //    {
            //        return Results.Ok(await new GetFilterUC(bannerContext).GetFilterByPosition(position));
            //    }
            //    catch (Exception ex)
            //    {
            //        return Results.BadRequest(ex.Message);
            //    }
            //}).RequireAuthorization();
        }

        public static void MapGetAllBanners(this WebApplication app)
        {
            //app.MapGet("/banners", async (ContentManagementContext bannerContext) =>
            //{
            //    try
            //    {
            //        return Results.Ok(await new GetFilterUC(bannerContext).GetAllBanner());
            //    }
            //    catch (Exception ex)
            //    {
            //        return Results.BadRequest(ex.Message);
            //    }
            //}).RequireAuthorization("OnlyAdmin");
        }
        #endregion

        #region Update Banner USECASE
        public static void MapUpdateBannerUseCaseAPIs(this WebApplication app)
        {
            MapUpdateBanner(app);
        }

        public static void MapUpdateBanner(this WebApplication app)
        {
            //app.MapPatch("/banners/{bannerID}/status", async (ContentManagementContext bannerContext, int bannerID, [FromBody] Filter newBanner) =>
            //{
            //    try
            //    {
            //        return Results.Ok(await new UpdateFilterUC(bannerContext).UpdateBanner(bannerID, newBanner));
            //    }
            //    catch (Exception ex)
            //    {
            //        return Results.BadRequest(ex.Message);
            //    }
            //}).RequireAuthorization("OnlyAdmin");
        }
        #endregion
    }
}
