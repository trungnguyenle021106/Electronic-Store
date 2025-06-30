using ApiDto.Response;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Usecases;
using ProductService.Domain.Entities;


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
            app.MapPost("/products", async (CreateProductUC createProductUC, Product product) =>
            {
                CreationResult<Product> result = await createProductUC.CreateProduct(product);

                if (result.IsSuccess)
                {
                    // 201 Created: Sản phẩm được tạo thành công
                    return Results.Created($"/products/{result.CreatedItem.ID}", result.CreatedItem);
                }
                else
                {
                    // Xử lý các loại lỗi khác nhau dựa trên ErrorType
                    return result.ErrorType switch
                    {
                        CreationErrorType.AlreadyExists => Results.Conflict(new { message = result.ErrorMessage }),
                        CreationErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                        CreationErrorType.RepositoryTypeMismatch => Results.Problem(
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Server Configuration Error",
                            detail: result.ErrorMessage
                        ),
                        CreationErrorType.InternalError => Results.Problem(
                             statusCode: StatusCodes.Status500InternalServerError,
                             title: "Internal Server Error",
                             detail: result.ErrorMessage
                         ),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Unknown Error",
                            detail: result.ErrorMessage
                        )
                    };
                }
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void AddPropertiesToProduct(this WebApplication app)
        {
            app.MapPost("/products/{productId}/productProperties", async (CreateProductUC createProductUC, int productId
                , [FromBody] List<int> productProperties) =>
            {

                CreationResult<ProductPropertyDetail> result = await createProductUC.AddPropertiesToProduct(productId, productProperties);

                if (result.IsSuccess)
                {
                    // Sản phẩm được tạo thành công
                    return Results.Ok(result.CreatedItems);
                }
                else
                {
                    // Xử lý các loại lỗi khác nhau dựa trên ErrorType
                    return result.ErrorType switch
                    {
                        CreationErrorType.AlreadyExists => Results.Conflict(new { message = result.ErrorMessage }),
                        CreationErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                        CreationErrorType.RepositoryTypeMismatch => Results.Problem(
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Server Configuration Error",
                            detail: "A server configuration error occurred. Please contact support."
                        ),
                        CreationErrorType.InternalError => Results.Problem(
                             statusCode: StatusCodes.Status500InternalServerError,
                             title: "Internal Server Error",
                             detail: "An internal server error occurred. Please try again later."
                         ),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Unknown Error",
                            detail: "An unexpected error occurred."
                        )
                    };
                }
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void CreateProductProperty(this WebApplication app)
        {
            app.MapPost("/productProperties", async (CreateProductUC createProductUC, ProductProperty productProperty) =>
            {
                CreationResult<ProductProperty> result = await createProductUC.CreateProductProperty(productProperty);

                if (result.IsSuccess)
                {
                    // 201 Created: Sản phẩm được tạo thành công
                    return Results.Created($"/productProperties/{result.CreatedItem.ID}", result.CreatedItem);
                }
                else
                {
                    // Xử lý các loại lỗi khác nhau dựa trên ErrorType
                    return result.ErrorType switch
                    {
                        CreationErrorType.AlreadyExists => Results.Conflict(new { message = result.ErrorMessage }),
                        CreationErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                        CreationErrorType.RepositoryTypeMismatch => Results.Problem(
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Server Configuration Error",
                            detail: result.ErrorMessage
                        ),
                        CreationErrorType.InternalError => Results.Problem(
                             statusCode: StatusCodes.Status500InternalServerError,
                             title: "Internal Server Error",
                             detail: result.ErrorMessage
                         ),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Unknown Error",
                            detail: result.ErrorMessage
                        )
                    };
                }
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
            app.MapGet("/products/{productID}", async (GetProductUC getProductUC, int productID) =>
            {
                QueryResult<Product> result = await getProductUC.GetProductByID(productID);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Item);
                }
                else
                {
                    return result.ErrorType switch
                    {
                        RetrievalErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                        RetrievalErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Unknown Error",
                            detail: result.ErrorMessage
                        )
                    };
                }
            });
        }

        public static void GetAllProducts(this WebApplication app)
        {
            app.MapGet("/products", async (GetProductUC getProductUC) =>
            {
                QueryResult<Product> result = await getProductUC.GetAllProducts();

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Items);
                }
                return Results.BadRequest(new { message = result.ErrorMessage });
            });
        }

        public static void GetAllProductProperties(this WebApplication app)
        {
            app.MapGet("/productProperties", async (GetProductUC getProductUC) =>
            {
                QueryResult<ProductProperty> result = await getProductUC.GetAllProductProperties();

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Items);
                }
                return Results.BadRequest(new { message = result.ErrorMessage });
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
            app.MapPut("/products/{productID}", async (UpdateProductUC updateProductUC, int productID,
                [FromBody] Product newProduct) =>
            {
                UpdateResult<Product> result = await updateProductUC.
                UpdateProduct(productID, newProduct);
                if (result.IsSuccess)
                {
                    return Results.Ok(result.UpdatedItem);
                }

                return result.ErrorType switch
                {
                    UpdateErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    UpdateErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: result.ErrorMessage
                    )
                };
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void UpdateProductProperty(this WebApplication app)
        {
            app.MapPatch("/productProperties/{productPropertyID}", async (UpdateProductUC updateProductUC,
                int productPropertyID, [FromBody] ProductProperty newProductProperty) =>
            {
                UpdateResult<ProductProperty> result = await updateProductUC.
                UpdateProductProperty(productPropertyID, newProductProperty);
                if (result.IsSuccess)
                {
                    return Results.Ok(result.UpdatedItem);
                }

                return result.ErrorType switch
                {
                    UpdateErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    UpdateErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: result.ErrorMessage
                    )
                };
            }).RequireAuthorization("OnlyAdmin");
        }
        #endregion

        #region Delete Product USECASE
        public static void MapDeleteProductUseCaseAPIs(this WebApplication app)
        {
            DeleteProduct(app);
            DeleteProductProperty(app);
            DeleteProductPropertyDetails(app);
        }

        public static void DeleteProduct(this WebApplication app)
        {
            app.MapDelete("/products", async (DeleteProductUC deleteProductUC,
            [FromBody] Product product) =>
            {
                DeletionResult<Product> result = await deleteProductUC.DeleteProduct(product);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.DeletedItem);
                }

                return result.ErrorType switch
                {
                    DeletionErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    DeletionErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: result.ErrorMessage
                    )
                };
            }).RequireAuthorization("OnlyAdmin");
        }


        public static void DeleteProductProperty(this WebApplication app)
        {
            app.MapDelete("/productProperties", async (DeleteProductUC deleteProductUC,
            [FromBody] ProductProperty productProperty) =>
            {
                DeletionResult<ProductProperty> result = await deleteProductUC.DeleteProductProperty(productProperty);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.DeletedItem);
                }

                return result.ErrorType switch
                {
                    DeletionErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    DeletionErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: result.ErrorMessage
                    )
                };
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void DeleteProductPropertyDetails(this WebApplication app)
        {
            app.MapDelete("/productPropertyDetails", async (DeleteProductUC deleteProductUC,
            [FromBody] List<ProductPropertyDetail> productPropertyDetails) =>
            {
                DeletionResult<ProductPropertyDetail> result = await deleteProductUC.DeleteProductPropertyDetails(productPropertyDetails);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.DeletedItem);
                }

                return result.ErrorType switch
                {
                    DeletionErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    DeletionErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: result.ErrorMessage
                    )
                };
            }).RequireAuthorization("OnlyAdmin");
        }
        #endregion
    }
}
