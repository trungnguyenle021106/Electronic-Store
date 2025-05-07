using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Usecases;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.DBContext;

namespace ProductService.Interface_Adapters.APIs
{
    public static class ProductAPI
    {
        public static void MapProductEndpoints(this WebApplication app)
        {
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
        }

        public static void CreateProduct(this WebApplication app)
        {
            app.MapPost("/products", async (ProductContext context, Product product) =>
            {
                return Results.Ok(await new CreateProductUC(context).CreateProduct(product));
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void AddPropertiesToProduct(this WebApplication app)
        {
            app.MapPost("/products/{productId}/productProperties", async (ProductContext context, int productId
                , [FromBody] List<int> productProperties) =>
            {
                return Results.Ok(await new CreateProductUC(context).
                    AddPropertiesToProduct(productId, productProperties));
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void CreateProductProperty(this WebApplication app)
        {
            app.MapPost("/productProperties", async (ProductContext context, ProductProperty productProperty) =>
            {
                return Results.Ok(await new CreateProductUC(context).CreateProductProperty(productProperty));
            }).RequireAuthorization("OnlyAdmin");
        }
        #endregion

        #region Get Product USECASE
        public static void MapGetProductUseCaseAPIs(this WebApplication app)
        {
            GetAllProductProperties(app);
            GetAllProducts(app);
            GetProductByID(app);
        }

        public static void GetProductByID(this WebApplication app)
        {
            app.MapGet("/products/{productID}", async (ProductContext context, int productID) =>
            {
                return Results.Ok(await new GetProductUC(context).GetProductByID(productID));
            });
        }

        public static void GetAllProducts(this WebApplication app)
        {
            app.MapGet("/products", async (ProductContext context) =>
            {
                return Results.Ok(await new GetProductUC(context).GetAllProducts());
            });
        }

        public static void GetAllProductProperties(this WebApplication app)
        {
            app.MapGet("/productProperties", async (ProductContext context) =>
            {
                return Results.Ok(await new GetProductUC(context).GetAllProductProperties());
            }).RequireAuthorization("OnlyAdmin");
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
            app.MapPatch("/products/{productID}", async (ProductContext context, int productID,
                [FromBody] Product newProduct) =>
            {
                return Results.Ok(await new UpdateProductUC(context).UpdateProduct(productID, newProduct));
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void UpdateProductProperty(this WebApplication app)
        {
            app.MapPatch("/productProperties/{productPropertyID}", async (ProductContext context,
                int productPropertyID, [FromBody] ProductProperty newProductProperty) =>
            {
                return Results.Ok(await new UpdateProductUC(context).
                    UpdateProductProperty(productPropertyID, newProductProperty));
            }).RequireAuthorization("OnlyAdmin");
        }
        #endregion

        #region Delete Product USECASE
        public static void MapDeleteProductUseCaseAPIs(this WebApplication app)
        {
            DeleteProduct(app);
        }

        public static void DeleteProduct(this WebApplication app)
        {
            app.MapDelete("/productPropertyDetails", async (ProductContext context,
            [FromBody] ProductPropertyDetail productPropertyDetail) =>
            {
                return Results.Ok(await new DeleteProductUC(context).
                   DeletePropertyDetail(productPropertyDetail));
            }).RequireAuthorization("OnlyAdmin");
        }
        #endregion
    }
}
