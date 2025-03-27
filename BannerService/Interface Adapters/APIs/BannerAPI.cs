
using BannerService.Application.Usecases;
using BannerService.Domain.Entities;
using BannerService.Infrastructure.DBContext;
using Microsoft.AspNetCore.Mvc;

namespace BannerService.Interface_Adapters.APIs
{
    public static class BannerAPI
    {
        public static void MapBannerEndpoints(this WebApplication app)
        {
            MapCreateBannerUseCaseAPIs(app);
            MapGetBannerUsecaseAPIs(app);
            MapUpdateBannerUseCaseAPIs(app);
        }

        #region Create Banner USECASE
        public static void MapCreateBannerUseCaseAPIs(this WebApplication app)
        {
            MapCreateBannerNormal(app);
        }

        public static void MapCreateBannerNormal(this WebApplication app)
        {
            app.MapPost("/banners", async (BannerContext bannerContext, Banner newBanner) =>
            {
                try
                {
                    return Results.Ok(await new CreateBannerUC(bannerContext).CreateBanner(newBanner));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
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
            app.MapGet("/banners/{bannerId}", async (BannerContext bannerContext, int bannerId) =>
            {
                try
                {
                    return Results.Ok(await new GetBannerUC(bannerContext).GetBannerByID(bannerId));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
        }

        public static void MapGetAllBanners(this WebApplication app)
        {
            app.MapGet("/banners", async (BannerContext bannerContext) =>
            {
                try
                {
                    return Results.Ok(await new GetBannerUC(bannerContext).GetAllBanner());
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
        }
        #endregion

        #region Update Banner USECASE
        public static void MapUpdateBannerUseCaseAPIs(this WebApplication app)
        {
            MapUpdateBanner(app);
        }

        public static void MapUpdateBanner(this WebApplication app)
        {
            app.MapPatch("/banners/{bannerID}/status", async (BannerContext bannerContext, int bannerID, [FromBody] Banner newBanner) =>
            {
                try
                {
                    return Results.Ok(await new UpdateBannerUC(bannerContext).UpdateBanner(bannerID, newBanner));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
        }
        #endregion
    }
}
