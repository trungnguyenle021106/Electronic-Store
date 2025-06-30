using ApiDto.Response;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.UnitOfWork;
using ProductService.Infrastructure.Data.Repositories;

namespace ProductService.Application.Usecases
{
    public class CreateProductUC
    {
        private readonly IUnitOfWork _UnitOfWork;
        public CreateProductUC(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }

        public async Task<CreationResult<Product>> CreateProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    return CreationResult<Product>
                        .Failure("No product provided to add.", CreationErrorType.ValidationError);
                }

                if (this._UnitOfWork.ProductRepository() is Repository<Product> productRepo)
                {
                    IQueryable<Product> query = productRepo.GetByNameQueryable(product.Name);
                    Product? existingProduct = await query.FirstOrDefaultAsync();

                    if (existingProduct != null)
                    {
                        // Lỗi nghiệp vụ: Sản phẩm đã tồn tại
                        return CreationResult<Product>.Failure(
                            $"Product with name '{product.Name}' already exists.",
                           CreationErrorType.AlreadyExists
                        );
                    }

                    await productRepo.AddAsync(product);
                    await _UnitOfWork.Commit();
                    return CreationResult<Product>.Success(product);
                }
                else
                {
                    // Lỗi cấu hình: Repository không phải loại mong đợi
                    return CreationResult<Product>.Failure(
                        "Internal configuration error: Product repository type mismatch.",
                        CreationErrorType.RepositoryTypeMismatch
                    );
                }
            }
            catch (Exception ex)
            {
                // Bắt các lỗi hệ thống không mong muốn khác (lỗi database, lỗi mạng, lỗi logic, ...)
                Console.Error.WriteLine($"Error creating product: {ex}");

                return CreationResult<Product>.Failure(
                    "An unexpected internal error occurred during product creation.",
                    CreationErrorType.InternalError
                );
            }
        }

        public async Task<CreationResult<ProductPropertyDetail>> AddPropertiesToProduct(int ProductID, List<int> listProductPropertyIDs)
        {
            try
            {
                if (listProductPropertyIDs == null || !listProductPropertyIDs.Any())
                {
                    return CreationResult<ProductPropertyDetail>.Failure("No product property IDs provided to add.", CreationErrorType.ValidationError);
                }

                IQueryable<ProductPropertyDetail> query = this._UnitOfWork.ProductPropertyDetailRepository().GetEntitiesByForeignKeyId(ProductID, "ProductID");
                List<int> existingPropertyIds = await query
                    .Select(ppd => ppd.ProductPropertyID)
                    .ToListAsync()
                    .ConfigureAwait(false);


                HashSet<int> existingPropertyIdsSet = new HashSet<int>();
                List<int> newProductPropertyIDsToAdd = new List<int>();
                foreach (int newId in listProductPropertyIDs.Distinct())
                {
                    if (!existingPropertyIdsSet.Contains(newId))
                    {
                        newProductPropertyIDsToAdd.Add(newId);
                    }
                }
                
                if (!newProductPropertyIDsToAdd.Any())
                {
                    return CreationResult<ProductPropertyDetail>.Failure(
                        "All provided product properties already exist for this product or were duplicates.",
                        CreationErrorType.AlreadyExists
                    );
                }

                // Các ProductPropertyDetail để thêm
                var entitiesToAdd = newProductPropertyIDsToAdd.Select(propId => new ProductPropertyDetail
                {
                    ProductID = ProductID,
                    ProductPropertyID = propId
                }).ToList();


                await this._UnitOfWork.ProductPropertyDetailRepository().AddRangeAsync(entitiesToAdd);
                await this._UnitOfWork.Commit();


                IQueryable<ProductPropertyDetail> finalQuery = this._UnitOfWork.ProductPropertyDetailRepository().GetEntitiesByForeignKeyId(ProductID, "ProductID");
                List<ProductPropertyDetail> updatedPropertiesOfProduct = await finalQuery.ToListAsync().ConfigureAwait(false);
                return CreationResult<ProductPropertyDetail>.Success(updatedPropertiesOfProduct);
            }
            catch (Exception ex)
            {

                // Bắt các lỗi hệ thống không mong muốn khác (lỗi database, lỗi mạng, lỗi logic, ...)
                Console.Error.WriteLine($"Error adding product property to product with ID {ProductID}: {ex}");

                return CreationResult<ProductPropertyDetail>.Failure(
                    "An unexpected internal error occurred while adding product properties.",
                    CreationErrorType.InternalError
                );
            }
        }

        public async Task<CreationResult<ProductProperty>> CreateProductProperty(ProductProperty productProperty)
        {
            try
            {
                if (this._UnitOfWork.ProductPropertyRepository() is Repository<ProductProperty> productPropertyRepo)
                {
                    IQueryable<ProductProperty> query = productPropertyRepo.GetByNameQueryable(productProperty.Name);
                    ProductProperty? existingProductProperty = await query.FirstOrDefaultAsync();

                    if (existingProductProperty != null)
                    {
                        // Lỗi nghiệp vụ: Thuộc tính sản phẩm đã tồn tại
                        return CreationResult<ProductProperty>.Failure(
                            $"Product property with name '{productProperty.Name}' already exists.",
                           CreationErrorType.AlreadyExists
                        );
                    }

                    await productPropertyRepo.AddAsync(productProperty);
                    await _UnitOfWork.Commit();
                    return CreationResult<ProductProperty>.Success(productProperty);
                }
                else
                {
                    // Lỗi cấu hình: Repository không phải loại mong đợi
                    return CreationResult<ProductProperty>.Failure(
                        "Internal configuration error: Product repository type mismatch.",
                        CreationErrorType.RepositoryTypeMismatch
                    );
                }
            }
            catch (Exception ex)
            {
                // Bắt các lỗi hệ thống không mong muốn khác (lỗi database, lỗi mạng, lỗi logic, ...)
                Console.Error.WriteLine($"Error creating product property: {ex}");

                return CreationResult<ProductProperty>.Failure(
                    "An unexpected internal error occurred during product property creation.",
                    CreationErrorType.InternalError
                );
            }
        }


        public async Task<CreationResult<ProductType>> CreateProductType(ProductType productType)
        {
            try
            {
                if (this._UnitOfWork.ProductTypeRepository() is Repository<ProductType> productTypeRepo)
                {
                    IQueryable<ProductType> query = productTypeRepo.GetByNameQueryable(productType.Name);
                    ProductType? existingProductType = await query.FirstOrDefaultAsync();

                    if (existingProductType != null)
                    {
                        // Lỗi nghiệp vụ: Loại sản phẩm đã tồn tại
                        return CreationResult<ProductType>.Failure(
                            $"Product type with name '{productType.Name}' already exists.",
                           CreationErrorType.AlreadyExists
                        );
                    }

                    await productTypeRepo.AddAsync(productType);
                    await _UnitOfWork.Commit();
                    return CreationResult<ProductType>.Success(productType);
                }
                else
                {
                    // Lỗi cấu hình: Repository không phải loại mong đợi
                    return CreationResult<ProductType>.Failure(
                        "Internal configuration error: Product repository type mismatch.",
                        CreationErrorType.RepositoryTypeMismatch
                    );
                }
            }
            catch (Exception ex)
            {
                // Bắt các lỗi hệ thống không mong muốn khác (lỗi database, lỗi mạng, lỗi logic, ...)
                Console.Error.WriteLine($"Error creating product type: {ex}");

                return CreationResult<ProductType>.Failure(
                    "An unexpected internal error occurred during product property creation.",
                    CreationErrorType.InternalError
                );
            }
        }

        public async Task<CreationResult<ProductBrand>> CreateProductBrand(ProductBrand productBrand)
        {
            try
            {
                if (this._UnitOfWork.ProductBrandRepository() is Repository<ProductBrand> productBrandRepo)
                {
                    IQueryable<ProductBrand> query = productBrandRepo.GetByNameQueryable(productBrand.Name);
                    ProductBrand? existingProductBrand = await query.FirstOrDefaultAsync();

                    if (existingProductBrand != null)
                    {
                        // Lỗi nghiệp vụ: Brand sản phẩm đã tồn tại
                        return CreationResult<ProductBrand>.Failure(
                            $"Product brand with name '{productBrand.Name}' already exists.",
                           CreationErrorType.AlreadyExists
                        );
                    }

                    await productBrandRepo.AddAsync(productBrand);
                    await _UnitOfWork.Commit();
                    return CreationResult<ProductBrand>.Success(productBrand);
                }
                else
                {
                    // Lỗi cấu hình: Repository không phải loại mong đợi
                    return CreationResult<ProductBrand>.Failure(
                        "Internal configuration error: Product repository type mismatch.",
                        CreationErrorType.RepositoryTypeMismatch
                    );
                }
            }
            catch (Exception ex)
            {
                // Bắt các lỗi hệ thống không mong muốn khác (lỗi database, lỗi mạng, lỗi logic, ...)
                Console.Error.WriteLine($"Error creating product brand: {ex}");

                return CreationResult<ProductBrand>.Failure(
                    "An unexpected internal error occurred during product property creation.",
                    CreationErrorType.InternalError
                );
            }
        }
    }
}
