using APIGateway.Application.Usecases;
using APIGateway.Infrastructure.DTO.ContentManagement;
using APIGateway.Infrastructure.DTO.ContentManagement.Request;
using APIGateway.Infrastructure.DTO.Product;
using CommonDto.ResultDTO;
using Microsoft.AspNetCore.Mvc;

namespace APIGateway.Interface_Adapters.APIs
{
    public static class GatewayAPI
    {

        public static void MapGatewayEndpoints(this WebApplication app)
        {
            app.MapWhen(context => context.Request.Path.StartsWithSegments("/filters"), appBuilder =>
            {
                appBuilder.UseRouting(); // Kích hoạt định tuyến
                appBuilder.UseAuthorization(); // Kích hoạt quyền truy cập

                // Map các endpoint liên quan đến Create Product Use Case
                appBuilder.UseEndpoints(endpoints =>
                {
                    endpoints.GWMapCreateUseCaseAPIs();
                    endpoints.GWMapGetUseCaseAPIs();
                });
            });
        }

        #region Create USECASE


        public static void GWMapCreateUseCaseAPIs(this IEndpointRouteBuilder endpoints)
        {
            CreateFilterAndFilterDetails(endpoints);
        }

        public static void CreateFilterAndFilterDetails(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/filters", async (GWCreateUC gWCreateUC, [FromBody] CreateFilterRequest createFilterRequest, HandleResultApi handleResultApi) =>
            {
                ServiceResult<Filter> result = await gWCreateUC.CreateFilterAndFilterDetails(createFilterRequest);
                return handleResultApi.MappingErrorHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        #endregion

        #region Get  USECASE
        public static void GWMapGetUseCaseAPIs(this IEndpointRouteBuilder endpoints)
        {
            GetAllPropertiesOfFilter(endpoints);
        }

       public static void GetAllPropertiesOfFilter(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/filters/{filterID}/properties", async (GWGetUC gWGetUC, int filterID, HandleResultApi handleResultApi) =>
            {
                ServiceResult<ProductProperty> result = await gWGetUC.getAllPropertiesOfFilter(filterID);
                return handleResultApi.MappingErrorHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        #endregion
    }
}
