using ApiDto.Response;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.UnitOfWork;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace ProductService.Application.Usecases
{
    public class GetProductUC
    {
        private readonly IUnitOfWork unitOfWork;
        public GetProductUC(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<QueryResult<Product>> GetProductByID(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return QueryResult<Product>.Failure("Product ID is invalid.", RetrievalErrorType.ValidationError);
                }

                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetByIdQueryable(id);
                Product? product = await query.FirstOrDefaultAsync();
                if (product == null)
                {
                    return QueryResult<Product>.Failure($"Product with ID : '{product.ID}' is not exist.",
                        RetrievalErrorType.NotFound);
                }

                return QueryResult<Product>.Success(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy sản phẩm có id : {id}, lỗi : {ex.Message}");
                return QueryResult<Product>.Failure("An unexpected internal error occurred while get product.",
                      RetrievalErrorType.InternalError);
            }
        }

        public async Task<QueryResult<Product>> GetAllProductByType(string productTypeName)
        {
            try
            {
                if (productTypeName != null)
                {
                    return QueryResult<Product>.Failure("Product type is invalid.", RetrievalErrorType.ValidationError);
                }

                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetAll();
                query = query
                .Join(
                  this.unitOfWork.ProductTypeRepository().GetAll(),
                  product => product.ProductTypeID,
                  productType => productType.ID,
                  (product, productType) => new { Product = product, ProductType = productType }
                  )
                .Where(item => item.ProductType.Name == productTypeName)
                .Select(item => item.Product);

                List<Product>? products = await query.ToListAsync();
                if (products == null)
                {
                    return QueryResult<Product>.Failure($"Product with type : '{productTypeName}' is not exist.",
                        RetrievalErrorType.NotFound);
                }

                return QueryResult<Product>.Success(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy sản phẩm có type : {productTypeName}, lỗi : {ex.Message}");
                return QueryResult<Product>.Failure("An unexpected internal error occurred while get product.",
                      RetrievalErrorType.InternalError);
            }
        }

        public async Task<QueryResult<Product>> GetAllProductByBrand(string productBrandName)
        {
            try
            {
                if (productBrandName != null)
                {
                    return QueryResult<Product>.Failure("Product type is invalid.", RetrievalErrorType.ValidationError);
                }

                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetAll();
                query = query
                .Join(
                  this.unitOfWork.ProductBrandRepository().GetAll(),
                  product => product.ProductBrandID,
                  productBrand => productBrand.ID,
                  (product, productBrand) => new { Product = product, ProductBrand = productBrand }
                  )
                .Where(item => item.ProductBrand.Name == productBrandName)
                .Select(item => item.Product);

                List<Product>? products = await query.ToListAsync();
                if (products == null)
                {
                    return QueryResult<Product>.Failure($"Product with name : '{productBrandName}' is not exist.",
                        RetrievalErrorType.NotFound);
                }

                return QueryResult<Product>.Success(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy sản phẩm có name : {productBrandName}, lỗi : {ex.Message}");
                return QueryResult<Product>.Failure("An unexpected internal error occurred while get product.",
                      RetrievalErrorType.InternalError);
            }
        }

        public async Task<QueryResult<Product>> GetAllProducts()
        {
            try
            {
                IQueryable<Product> productsQuery = this.unitOfWork.ProductRepository().GetAll();
                List<Product> productsList = await productsQuery
                .Select(p => new Product
                {
                    ID = p.ID,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    Image = p.Image,
                    Description = p.Description,
                    Price = p.Price,
                    Status = p.Status
                })
                .ToListAsync();
                return QueryResult<Product>.Success(productsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách sản phẩm : {ex.Message}");
                return QueryResult<Product>.Failure("An unexpected internal error occurred while get products.",
                      RetrievalErrorType.InternalError);
            }
        }

        public async Task<QueryResult<ProductProperty>> GetAllProductProperties()
        {
            try
            {
                IQueryable<ProductProperty> productPropertiesQuery = this.unitOfWork.ProductPropertyRepository().GetAll();
                List<ProductProperty> productPropertiesList = await productPropertiesQuery.ToListAsync().ConfigureAwait(false);
                return QueryResult<ProductProperty>.Success(productPropertiesList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách thuộc tính sản phẩm : {ex.Message}");
                return QueryResult<ProductProperty>.Failure("An unexpected internal error occurred while get product properties.",
                      RetrievalErrorType.InternalError);
            }
        }

        public async Task<QueryResult<ProductType>> GetAllProductType()
        {
            try
            {
                IQueryable<ProductType> query = this.unitOfWork.ProductTypeRepository().GetAll();
                List<ProductType> productTypes = await query.ToListAsync();

                return QueryResult<ProductType>.Success(productTypes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách loại sản phẩm, lỗi : {ex.Message}");
                return QueryResult<ProductType>.Failure("An unexpected internal error occurred while get product.",
                      RetrievalErrorType.InternalError);
            }
        }

        public async Task<QueryResult<ProductBrand>> GetAllProductBrand()
        {
            try
            {
                IQueryable<ProductBrand> query = this.unitOfWork.ProductBrandRepository().GetAll();
                List<ProductBrand> productBrands = await query.ToListAsync();

                return QueryResult<ProductBrand>.Success(productBrands);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách brand sản phẩm, lỗi : {ex.Message}");
                return QueryResult<ProductBrand>.Failure("An unexpected internal error occurred while get product.",
                      RetrievalErrorType.InternalError);
            }
        }

        public async Task<QueryResult<Product>> GetAllProductByTypeAndProperty(string productTypeName, ProductProperty productProperty)
        {
            try
            {
                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetAll();
                query = query.
                    Join(this.unitOfWork.ProductPropertyDetailRepository().GetAll(),
                    product => product.ID,
                    productPropertyDetail => productPropertyDetail.ProductID,
                    (product, productPropertyDetail) => new
                    {
                        Product = product,
                        ProductPropertyDetail = productPropertyDetail
                    }).
                    Join(this.unitOfWork.ProductPropertyRepository().GetAll(),
                      joinedItem => joinedItem.ProductPropertyDetail.ProductPropertyID,
                      productProperty => productProperty.ID,
                      (joinedItem, productProperty) => new
                      {
                          Product = joinedItem.Product,
                          ProductPropertyDetail = joinedItem.ProductPropertyDetail,
                          ProductProperty = productProperty
                      }).
                      Where(item =>
                            item.ProductProperty.Name == productTypeName &&
                            item.ProductProperty.Name == productProperty.Name &&
                            item.ProductProperty.Description == productProperty.Description
                      ).
                      Select(item => item.Product);
                List<Product> products = await query.ToListAsync();

                return QueryResult<Product>.Success(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách sản phẩm, lỗi : {ex.Message}");
                return QueryResult<Product>.Failure("An unexpected internal error occurred while get product.",
                      RetrievalErrorType.InternalError);
            }
        }

        public async Task<QueryResult<Product>> GetAllProductByTypeAndBrand(string productTypeName, string productBrandName)
        {
            try
            {
                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetAll();
                query = query.
                    Join(this.unitOfWork.ProductBrandRepository().GetAll(),
                    product => product.ProductBrandID,
                    productBrand => productBrand.ID,
                    (product, productBrand) => new
                    {
                        Product = product,
                        ProductBrand = productBrand
                    }).
                      Join(this.unitOfWork.ProductTypeRepository().GetAll(),
                    joinedItem => joinedItem.Product.ProductTypeID,
                    productType => productType.ID,
                    (joinedItem, productType) => new
                    {
                        Product = joinedItem.Product,
                        ProductBrand = joinedItem.ProductBrand,
                        ProductType = productType
                    }).
                      Where(item =>
                            item.ProductBrand.Name == productBrandName &&
                            item.ProductType.Name == productTypeName
                      ).
                      Select(item => item.Product);
                List<Product> products = await query.ToListAsync();

                return QueryResult<Product>.Success(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách sản phẩm, lỗi : {ex.Message}");
                return QueryResult<Product>.Failure("An unexpected internal error occurred while get product.",
                      RetrievalErrorType.InternalError);
            }
        }
    }
}
