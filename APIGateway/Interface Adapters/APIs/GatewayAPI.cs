using APIGateway.Application.Usecases;
using APIGateway.Infrastructure.DTO.ContentManagement;
using APIGateway.Infrastructure.DTO.ContentManagement.Request;
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
                });
            });
            //GWMapCreateUseCaseAPIs(app);
            //MapGetProductUseCaseAPIs(app);
            //MapUpdateProductUseCaseAPIs(app);
            //MapDeleteProductUseCaseAPIs(app);
        }

        #region Create Product USECASE


        public static void GWMapCreateUseCaseAPIs(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/filters", async (GWCreateUC gWCreateUC, [FromBody] CreateFilterRequest createFilterRequest, HandleResultApi handleResultApi) =>
            {
                ServiceResult<Filter> result = await gWCreateUC.CreateFilterAndFilterDetails(createFilterRequest);
                return handleResultApi.MappingErrorHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void CreateFilterAndFilterDetails(this WebApplication app)
        {
            app.MapPost("/filters", async (GWCreateUC gWCreateUC, [FromBody] CreateFilterRequest createFilterRequest, HandleResultApi handleResultApi) =>
            {
              ServiceResult<Filter> result = await gWCreateUC.CreateFilterAndFilterDetails(createFilterRequest);
                return handleResultApi.MappingErrorHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        #endregion

        #region Get Product USECASE
        public static void MapGetProductUseCaseAPIs(this WebApplication app)
        {
            GetProductByID(app);
        }

        public static void GetProductByID(this WebApplication app)
        {
            //app.MapGet("/products/{productID}", async (GetProductUC getProductUC, int productID) =>
            //{
            //    QueryResult<Product> result = await getProductUC.GetProductByID(productID);

            //    if (result.IsSuccess)
            //    {
            //        return Results.Ok(result.Item);
            //    }
            //    else
            //    {
            //        return result.ErrorType switch
            //        {
            //            RetrievalErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
            //            RetrievalErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
            //            _ => Results.Problem(
            //                statusCode: StatusCodes.Status500InternalServerError,
            //                title: "Unknown Error",
            //                detail: result.ErrorMessage
            //            )
            //        };
            //    }
            //});
        }

        #endregion

        #region Update Product USECASE
        public static void MapUpdateProductUseCaseAPIs(this WebApplication app)
        {
            UpdateProduct(app);
            UpdateProductProperty(app);
        }

        public static void UpdateProduct(this WebApplication app)
        {
            //app.MapPut("/products/{productID}", async (UpdateProductUC updateProductUC, int productID,
            //    [FromBody] Product newProduct) =>
            //{
            //    UpdateResult<Product> result = await updateProductUC.
            //    UpdateProduct(productID, newProduct);
            //    if (result.IsSuccess)
            //    {
            //        return Results.Ok(result.UpdatedItem);
            //    }

            //    return result.ErrorType switch
            //    {
            //        UpdateErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
            //        UpdateErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
            //        _ => Results.Problem(
            //            statusCode: StatusCodes.Status500InternalServerError,
            //            title: "Unknown Error",
            //            detail: result.ErrorMessage
            //        )
            //    };
            //}).RequireAuthorization("OnlyAdmin");
        }

        public static void UpdateProductProperty(this WebApplication app)
        {
            //app.MapPatch("/productProperties/{productPropertyID}", async (UpdateProductUC updateProductUC,
            //    int productPropertyID, [FromBody] ProductProperty newProductProperty) =>
            //{
            //    UpdateResult<ProductProperty> result = await updateProductUC.
            //    UpdateProductProperty(productPropertyID, newProductProperty);
            //    if (result.IsSuccess)
            //    {
            //        return Results.Ok(result.UpdatedItem);
            //    }

            //    return result.ErrorType switch
            //    {
            //        UpdateErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
            //        UpdateErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
            //        _ => Results.Problem(
            //            statusCode: StatusCodes.Status500InternalServerError,
            //            title: "Unknown Error",
            //            detail: result.ErrorMessage
            //        )
            //    };
            //}).RequireAuthorization("OnlyAdmin");
        }
        #endregion

        #region Delete Product USECASE
        public static void MapDeleteProductUseCaseAPIs(this WebApplication app)
        {
            DeleteProduct(app);
        
        }

        public static void DeleteProduct(this WebApplication app)
        {
            //app.MapDelete("/products", async (DeleteProductUC deleteProductUC,
            //[FromBody] Product product) =>
            //{
            //    DeletionResult<Product> result = await deleteProductUC.DeleteProduct(product);

            //    if (result.IsSuccess)
            //    {
            //        return Results.Ok(result.DeletedItem);
            //    }

            //    return result.ErrorType switch
            //    {
            //        DeletionErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
            //        DeletionErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
            //        _ => Results.Problem(
            //            statusCode: StatusCodes.Status500InternalServerError,
            //            title: "Unknown Error",
            //            detail: result.ErrorMessage
            //        )
            //    };
            //}).RequireAuthorization("OnlyAdmin");
        }
        #endregion
    }
}
