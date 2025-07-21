using Amazon.Util.Internal;
using CommonDto.ResultDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ProductService.Application.Usecases;
using ProductService.Domain.DTO;
using ProductService.Domain.DTO.Request;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Socket;

namespace ProductService.Interface_Adapters.APIs
{
    public static class ProductAPI
    {

        public static void MapProductEndpoints(this WebApplication app)
        {
            app.MapHub<ProductPropertyHub>("/productPropertyHub")
             .RequireAuthorization("OnlyAdmin");

            app.MapHub<ProductTypeHub>("/productTypeHub")
            .RequireAuthorization("OnlyAdmin");

            app.MapHub<ProductBrandHub>("/productBrandHub")
            .RequireAuthorization("OnlyAdmin");

            MapCreateProductUseCaseAPIs(app);
            MapGetProductUseCaseAPIs(app);
            MapUpdateProductUseCaseAPIs(app);
            MapDeleteProductUseCaseAPIs(app);
        }

        #region Create Product USECASE
        public static void MapCreateProductUseCaseAPIs(this WebApplication app)
        {
            CreateProduct(app);
            AddPropertiesToProduct(app);
            CreateProductProperty(app);
            CreateProductBrand(app);
            CreateProductType(app);
        }

        public static void CreateProductBrand(this WebApplication app)
        {
            app.MapPost("/product-brands", async (CreateProductUC createProductUC, IHubContext<ProductBrandHub> ProductBrandHubContext,
                [FromBody] List<ProductBrand> listProductBrands, HandleResultApi handleResultApi) =>
            {
                ServiceResult<ProductBrand> result = await createProductUC.CreateMultiProductBrands(listProductBrands);
                if (result.IsSuccess)
                {
                    await ProductBrandHubContext.Clients.All.SendAsync(
                       "ProductBrandChanged", // Tên event mà client Angular sẽ lắng nghe
                       result.ListItem[0],
                       "ProductBrandAdded" // Thêm một tin nhắn kèm theo
                       );
                }
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void CreateProductType(this WebApplication app)
        {
            app.MapPost("/product-types", async (CreateProductUC createProductUC, IHubContext<ProductTypeHub> ProductTypeHubContext,
                [FromBody] List<ProductType> listProductTypes, HandleResultApi handleResultApi) =>
            {
                ServiceResult<ProductType> result = await createProductUC.CreateMultiProductTypes(listProductTypes);
                if (result.IsSuccess)
                {
                    await ProductTypeHubContext.Clients.All.SendAsync(
                       "ProductTypeChanged",
                       result.ListItem[0],
                       "ProductTypeAdded"
                       );
                }
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void CreateProduct(this WebApplication app)
        {
            app.MapPost("/products", async (CreateProductUC createProductUC, [FromForm] CreateUpdateProductRequest createProductReq,
                HandleResultApi handleResultApi, ManageProductImagesUC manageProductImagesUC) =>
            {
                Product productEntity = new Product
                {
                    Name = createProductReq.Name,
                    Quantity = createProductReq.Quantity,
                    ProductBrandID = createProductReq.ProductBrandID,
                    ProductTypeID = createProductReq.ProductTypeID,
                    Description = createProductReq.Description,
                    Price = createProductReq.Price,
                    Status = createProductReq.Status,
                };
                ServiceResult<Product> result = await createProductUC.
                CreateProduct(productEntity, createProductReq.ProductPropertyIDs, createProductReq.File);
                return handleResultApi.MapServiceResultToHttp(result);
            }).DisableAntiforgery().RequireAuthorization("OnlyAdmin");
        }

        public static void AddPropertiesToProduct(this WebApplication app)
        {
            app.MapPost("/products/{productId}/product-properties", async (CreateProductUC createProductUC, HandleResultApi handleResultApi, int productId
                , [FromBody] List<int> productProperties) =>
            {
                ServiceResult<ProductPropertyDetail> result = await createProductUC.AddPropertiesToProduct(productId, productProperties);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void CreateProductProperty(this WebApplication app)
        {
            app.MapPost("/product-properties", async (CreateProductUC createProductUC, IHubContext<ProductPropertyHub> productPropertyHubContext,
                [FromBody] List<ProductProperty> listProductProperties, HandleResultApi handleResultApi) =>
            {
                ServiceResult<ProductProperty> result = await createProductUC.CreateMultiProductProperties(listProductProperties);
                if (result.IsSuccess)
                {
                    await productPropertyHubContext.Clients.All.SendAsync(
                       "ProductPropertyChanged",
                       result.ListItem[0],
                       "ProductPropertyAdded"
                       );
                }

                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        #endregion

        #region Get Product USECASE
        public static void MapGetProductUseCaseAPIs(this WebApplication app)
        {
            GetPagedProductTypes(app);
            GetPagedProductBrands(app);
            GetProductByID(app);
            GetPagedProducts(app);
            GetPagedProductProperties(app);
            GetProductPropertyNames(app);
            GetAllPropertiesOfProduct(app);
        }

        public static void GetAllPropertiesOfProduct(this WebApplication app)
        {
            app.MapGet("/products/{productID}/product-properties", async (GetProductUC getProductUC, HandleResultApi handleResultApi, int productID) =>
            {
                ServiceResult<ProductProperty> result = await getProductUC.GetAllPropertiesOfProduct(productID);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void GetProductByID(this WebApplication app)
        {
            app.MapGet("/products/{productID}", async (GetProductUC getProductUC, int productID, HandleResultApi handleResultApi) =>
            {
                ServiceResult<Product> result = await getProductUC.GetProductByID(productID);
                return handleResultApi.MapServiceResultToHttp(result);
            });
        }

        public static void GetProductPropertyNames(this WebApplication app)
        {
            app.MapGet("/product-property-names", async (GetProductUC getProductUC, HandleResultApi handleResultApi) =>
            {
                ServiceResult<string> result = await getProductUC.GetAllUniquePropertyNames();
                return handleResultApi.MapServiceResultToHttp(result);

            }).RequireAuthorization();
        }

        public static void GetPagedProducts(this WebApplication app)
        {
            app.MapGet("/products", async (GetProductUC getProductUC, HandleResultApi handleResultApi,
                [FromQuery] int page,
                [FromQuery] int pageSize,
                [FromQuery] string? searchText,
                [FromQuery] string? filterBrand, [FromQuery] string? filterType, [FromQuery] string? filterStatus) =>
            {
                ServiceResult<PagedResult<ProductDTO>> result = await getProductUC.
                GetPagedProducts(page, pageSize, searchText, filterBrand, filterType, filterStatus);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }


        public static void GetPagedProductProperties(this WebApplication app)
        {
            app.MapGet("/product-properties", async (
                GetProductUC getProductUC,
                HandleResultApi handleResultApi,
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromQuery] string? searchText,
                [FromQuery] string? filter
            ) =>
            {
                ServiceResult<PagedResult<ProductProperty>> result =
                    await getProductUC.GetPagedProductProperties(page, pageSize, searchText, filter);

                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin"); // Giữ nguyên chính sách ủy quyền
        }

        public static void GetPagedProductTypes(this WebApplication app)
        {
            app.MapGet("/product-types", async (
                GetProductUC getProductUC,
                HandleResultApi handleResultApi,
                [FromQuery] int page,
                [FromQuery] int pageSize,
                [FromQuery] string? searchText,
                [FromQuery] string? filter
            ) =>
            {
                ServiceResult<PagedResult<ProductType>> result =
                    await getProductUC.GetPagedProductTypes(page, pageSize, searchText, filter);

                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin"); // Giữ nguyên chính sách ủy quyền
        }

        public static void GetPagedProductBrands(this WebApplication app)
        {
            app.MapGet("/product-brands", async (
                GetProductUC getProductUC,
                HandleResultApi handleResultApi,
                [FromQuery] int page,
                [FromQuery] int pageSize,
                [FromQuery] string? searchText,
                [FromQuery] string? filter
            ) =>
            {
                ServiceResult<PagedResult<ProductBrand>> result =
                    await getProductUC.GetPagedProductBrands(page, pageSize, searchText, filter);

                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin"); // Giữ nguyên chính sách ủy quyền
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
            app.MapPatch("/products/{productID}", async (UpdateProductUC updateProductUC, [FromForm] CreateUpdateProductRequest updateProductReq,
                HandleResultApi handleResultApi, ManageProductImagesUC manageProductImagesUC, int productID) =>
            {
                Product productEntity = new Product
                {
                    ID = productID,
                    Name = updateProductReq.Name,
                    Quantity = updateProductReq.Quantity,
                    ProductBrandID = updateProductReq.ProductBrandID,
                    ProductTypeID = updateProductReq.ProductTypeID,
                    Description = updateProductReq.Description,
                    Price = updateProductReq.Price,
                    Status = updateProductReq.Status,
                    Image = updateProductReq.Image
                };
                ServiceResult<Product> result = await updateProductUC.UpdateProduct(productEntity, updateProductReq.ProductPropertyIDs, updateProductReq.File);
                return handleResultApi.MapServiceResultToHttp(result);
            }).DisableAntiforgery().RequireAuthorization("OnlyAdmin");
        }

        public static void UpdateProductType(this WebApplication app)
        {
            app.MapPatch("/product-types/{productTypeID}", async (UpdateProductUC updateProductUC, HandleResultApi handleResultApi,
                int productTypeID, [FromBody] ProductType newProductType, IHubContext<ProductTypeHub> ProductTypeHubContext) =>
            {
                ServiceResult<ProductType> result = await updateProductUC.UpdateProductType(productTypeID, newProductType);
                if (result.IsSuccess)
                {
                    await ProductTypeHubContext.Clients.All.SendAsync(
                       "ProductTypeChanged", // Tên event mà client Angular sẽ lắng nghe
                       result.Item,
                       "ProductTypeUpdated" // Thêm một tin nhắn kèm theo
                       );
                }

                return handleResultApi.MapServiceResultToHttp(result);

            }).RequireAuthorization("OnlyAdmin");
        }

        public static void UpdateProductBrand(this WebApplication app)
        {
            app.MapPatch("/product-brands/{productBrandID}", async (UpdateProductUC updateProductUC, HandleResultApi handleResultApi,
                int productBrandID, [FromBody] ProductBrand newProductBrand, IHubContext<ProductBrandHub> productBrandHubContext) =>
            {
                ServiceResult<ProductBrand> result = await updateProductUC.UpdateProductBrand(productBrandID, newProductBrand);
                if (result.IsSuccess)
                {
                    await productBrandHubContext.Clients.All.SendAsync(
                       "ProductBrandChanged", // Tên event mà client Angular sẽ lắng nghe
                       result.Item,
                       "ProductBrandUpdated" // Thêm một tin nhắn kèm theo
                       );
                }

                return handleResultApi.MapServiceResultToHttp(result);

            }).RequireAuthorization("OnlyAdmin");
        }

        public static void UpdateProductProperty(this WebApplication app)
        {
            app.MapPatch("/product-properties/{productPropertyID}", async (UpdateProductUC updateProductUC, HandleResultApi handleResultApi,
                int productPropertyID, [FromBody] ProductProperty newProductProperty, IHubContext<ProductPropertyHub> productPropertyHubContext) =>
            {
                ServiceResult<ProductProperty> result = await updateProductUC.UpdateProductProperty(productPropertyID, newProductProperty);
                if (result.IsSuccess)
                {
                    await productPropertyHubContext.Clients.All.SendAsync(
                       "ProductPropertyChanged",
                       result.Item,
                       "ProductPropertyUpdated" // Thêm một tin nhắn kèm theo
                       );
                }

                return handleResultApi.MapServiceResultToHttp(result);

            }).RequireAuthorization("OnlyAdmin");
        }
        #endregion

        #region Delete Product USECASE
        public static void MapDeleteProductUseCaseAPIs(this WebApplication app)
        {
            DeleteProduct(app);
            DeleteProductProperty(app);
            DeleteProductPropertyDetails(app);
            DeleteProductType(app);
            DeleteProductBrand(app);
        }

        public static void DeleteProduct(this WebApplication app)
        {
            app.MapDelete("/products/{productID}", async (DeleteProductUC deleteProductUC, HandleResultApi handleResultApi,
           int productID) =>
            {
                ServiceResult<Product> result = await deleteProductUC.DeleteProduct(productID);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void DeleteProductType(this WebApplication app)
        {
            app.MapDelete("/product-types/{id}", async (DeleteProductUC deleteProductUC, HandleResultApi handleResultApi,
            int id, IHubContext<ProductTypeHub> ProductTypeHubContext) =>
            {
                ServiceResult<ProductType> result = await deleteProductUC.DeleteProductType(id);
                if (result.IsSuccess)
                {
                    await ProductTypeHubContext.Clients.All.SendAsync(
                       "ProductTypeChanged", // Tên event mà client Angular sẽ lắng nghe
                       result.Item,
                       "ProductTypeDeleted" // Thêm một tin nhắn kèm theo
                       );
                }
                return handleResultApi.MapServiceResultToHttp(result);

            }).RequireAuthorization("OnlyAdmin");
        }

        public static void DeleteProductBrand(this WebApplication app)
        {
            app.MapDelete("/product-brands/{id}", async (DeleteProductUC deleteProductUC, HandleResultApi handleResultApi,
            int id, IHubContext<ProductBrandHub> ProductBrandHubContext) =>
            {
                ServiceResult<ProductBrand> result = await deleteProductUC.DeleteProductBrand(id);
                if (result.IsSuccess)
                {
                    await ProductBrandHubContext.Clients.All.SendAsync(
                       "ProductBrandChanged", // Tên event mà client Angular sẽ lắng nghe
                       result.Item,
                       "ProductBrandDeleted" // Thêm một tin nhắn kèm theo
                       );
                }
                return handleResultApi.MapServiceResultToHttp(result);

            }).RequireAuthorization("OnlyAdmin");
        }


        public static void DeleteProductProperty(this WebApplication app)
        {
            app.MapDelete("/product-properties/{id}", async (DeleteProductUC deleteProductUC, HandleResultApi handleResultApi,
            int id, IHubContext<ProductPropertyHub> productPropertyHubContext) =>
            {
                ServiceResult<ProductProperty> result = await deleteProductUC.DeleteProductProperty(id);
                if (result.IsSuccess)
                {
                    await productPropertyHubContext.Clients.All.SendAsync(
                       "ProductPropertyChanged", // Tên event mà client Angular sẽ lắng nghe
                       result.Item,
                       "ProductPropertyDeleted" // Thêm một tin nhắn kèm theo
                       );
                }
                return handleResultApi.MapServiceResultToHttp(result);

            }).RequireAuthorization("OnlyAdmin");
        }

        public static void DeleteProductPropertyDetails(this WebApplication app)
        {
            app.MapDelete("/productPropertyDetails", async (DeleteProductUC deleteProductUC, HandleResultApi handleResultApi,
            [FromBody] List<ProductPropertyDetail> productPropertyDetails) =>
            {
                ServiceResult<ProductPropertyDetail> result = await deleteProductUC.DeleteProductPropertyDetails(productPropertyDetails);
                return handleResultApi.MapServiceResultToHttp(result);
                //if (result.IsSuccess)
                //{
                //    return Results.Ok(result.ListItem);
                //}

                //return result.ServiceErrorType switch
                //{
                //    ServiceErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                //    ServiceErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                //    _ => Results.Problem(
                //        statusCode: StatusCodes.Status500InternalServerError,
                //        title: "Unknown Error",
                //        detail: result.ErrorMessage
                //    )
                //};
            }).RequireAuthorization("OnlyAdmin");
        }
        #endregion
    }
}
