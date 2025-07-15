using CommonDto.ResultDTO;
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

        public async Task<ServiceResult<Product>> CreateProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    return ServiceResult<Product>
                        .Failure("No product provided to add.", ServiceErrorType.ValidationError);
                }

                if (this._UnitOfWork.ProductRepository() is Repository<Product> productRepo)
                {
                    IQueryable<Product> query = productRepo.GetByNameQueryable(product.Name);
                    Product? existingProduct = await query.FirstOrDefaultAsync();

                    if (existingProduct != null)
                    {
                        // Business error: Product already exists
                        return ServiceResult<Product>.Failure(
                            $"Product with name '{product.Name}' already exists.",
                            ServiceErrorType.AlreadyExists
                        );
                    }

                    await productRepo.AddAsync(product);
                    await _UnitOfWork.Commit();
                    return ServiceResult<Product>.Success(product);
                }
                else
                {
                    // Configuration error: Repository type mismatch
                    return ServiceResult<Product>.Failure(
                        "Internal configuration error: Product repository type mismatch.",
                        ServiceErrorType.RepositoryTypeMismatch
                    );
                }
            }
            catch (Exception ex)
            {
                // Catch unexpected system errors (database, network, logic, ...)
                Console.Error.WriteLine($"Error creating product: {ex}");

                return ServiceResult<Product>.Failure(
                    "An unexpected internal error occurred during product creation.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<ProductPropertyDetail>> AddPropertiesToProduct(int ProductID, List<int> listProductPropertyIDs)
        {
            try
            {
                if (listProductPropertyIDs == null || !listProductPropertyIDs.Any())
                {
                    return ServiceResult<ProductPropertyDetail>.Failure("No product property IDs provided to add.", ServiceErrorType.ValidationError);
                }

                IQueryable<ProductPropertyDetail> query = this._UnitOfWork.ProductPropertyDetailRepository().GetEntitiesByForeignKeyId(ProductID, "ProductID");
                List<int> existingPropertyIds = await query
                    .Select(ppd => ppd.ProductPropertyID)
                    .ToListAsync()
                    .ConfigureAwait(false);

                HashSet<int> existingPropertyIdsSet = new HashSet<int>(existingPropertyIds);
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
                    return ServiceResult<ProductPropertyDetail>.Failure(
                        "All provided product properties already exist for this product or were duplicates.",
                        ServiceErrorType.AlreadyExists
                    );
                }

                // Entities to add
                var entitiesToAdd = newProductPropertyIDsToAdd.Select(propId => new ProductPropertyDetail
                {
                    ProductID = ProductID,
                    ProductPropertyID = propId
                }).ToList();

                await this._UnitOfWork.ProductPropertyDetailRepository().AddRangeAsync(entitiesToAdd);
                await this._UnitOfWork.Commit();

                IQueryable<ProductPropertyDetail> finalQuery = this._UnitOfWork.ProductPropertyDetailRepository().GetEntitiesByForeignKeyId(ProductID, "ProductID");
                List<ProductPropertyDetail> updatedPropertiesOfProduct = await finalQuery.ToListAsync().ConfigureAwait(false);
                return ServiceResult<ProductPropertyDetail>.Success(updatedPropertiesOfProduct);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error adding product property to product with ID {ProductID}: {ex}");

                return ServiceResult<ProductPropertyDetail>.Failure(
                    "An unexpected internal error occurred while adding product properties.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<ProductProperty>> CreateMultiProductProperties(List<ProductProperty> listProductProperties)
        {
            if (listProductProperties == null || !listProductProperties.Any())
            {
                return ServiceResult<ProductProperty>
                    .Failure("No product properties provided to add.", ServiceErrorType.ValidationError);
            }

            try
            {
                await this._UnitOfWork.ProductPropertyRepository().AddRangeAsync(listProductProperties);
                await _UnitOfWork.Commit();
                return ServiceResult<ProductProperty>.Success(listProductProperties);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating multiple ProductBrand: {ex}");

                return ServiceResult<ProductProperty>.Failure(
                    "An unexpected internal error occurred during product property creation.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<ProductBrand>> CreateMultiProductBrands(List<ProductBrand> listProductBrands)
        {
            if (listProductBrands == null || !listProductBrands.Any())
            {
                return ServiceResult<ProductBrand>
                    .Failure("No ProductBrand provided to add.", ServiceErrorType.ValidationError);
            }

            try
            {
                await this._UnitOfWork.ProductBrandRepository().AddRangeAsync(listProductBrands);
                await _UnitOfWork.Commit();
                return ServiceResult<ProductBrand>.Success(listProductBrands);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating multiple ProductBrand: {ex}");

                return ServiceResult<ProductBrand>.Failure(
                    "An unexpected internal error occurred during ProductBrand creation.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<ProductType>> CreateMultiProductTypes(List<ProductType> listProductTypes)
        {
            if (listProductTypes == null || !listProductTypes.Any())
            {
                return ServiceResult<ProductType>
                    .Failure("No ProductType provided to add.", ServiceErrorType.ValidationError);
            }

            try
            {
                await this._UnitOfWork.ProductTypeRepository().AddRangeAsync(listProductTypes);
                await _UnitOfWork.Commit();
                return ServiceResult<ProductType>.Success(listProductTypes);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating multiple product properties: {ex}");

                return ServiceResult<ProductType>.Failure(
                    "An unexpected internal error occurred during ProductType creation.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<ProductProperty>> CreateProductProperty(ProductProperty productProperty)
        {
            if (productProperty == null)
            {
                return ServiceResult<ProductProperty>
                    .Failure("No product property provided to add.", ServiceErrorType.ValidationError);
            }

            try
            {
              
                await this._UnitOfWork.ProductPropertyRepository().AddAsync(productProperty);
                await _UnitOfWork.Commit();
                return ServiceResult<ProductProperty>.Success(productProperty);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating product property: {ex}");

                return ServiceResult<ProductProperty>.Failure(
                    "An unexpected internal error occurred during product property creation.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<ProductType>> CreateProductType(ProductType productType)
        {
            try
            {
                if (this._UnitOfWork.ProductTypeRepository() is Repository<ProductType> productTypeRepo)
                {
                    IQueryable<ProductType> query = productTypeRepo.GetByNameQueryable(productType.Name);
                    ProductType? existingProductType = await query.FirstOrDefaultAsync();

                    if (existingProductType != null)
                    {
                        // Business error: Product type already exists
                        return ServiceResult<ProductType>.Failure(
                            $"Product type with name '{productType.Name}' already exists.",
                            ServiceErrorType.AlreadyExists
                        );
                    }

                    await productTypeRepo.AddAsync(productType);
                    await _UnitOfWork.Commit();
                    return ServiceResult<ProductType>.Success(productType);
                }
                else
                {
                    // Configuration error: Repository type mismatch
                    return ServiceResult<ProductType>.Failure(
                        "Internal configuration error: Product repository type mismatch.",
                        ServiceErrorType.RepositoryTypeMismatch
                    );
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating product type: {ex}");

                return ServiceResult<ProductType>.Failure(
                    "An unexpected internal error occurred during product property creation.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<ProductBrand>> CreateProductBrand(ProductBrand productBrand)
        {
            try
            {
                if (this._UnitOfWork.ProductBrandRepository() is Repository<ProductBrand> productBrandRepo)
                {
                    IQueryable<ProductBrand> query = productBrandRepo.GetByNameQueryable(productBrand.Name);
                    ProductBrand? existingProductBrand = await query.FirstOrDefaultAsync();

                    if (existingProductBrand != null)
                    {
                        // Business error: Product brand already exists
                        return ServiceResult<ProductBrand>.Failure(
                            $"Product brand with name '{productBrand.Name}' already exists.",
                            ServiceErrorType.AlreadyExists
                        );
                    }

                    await productBrandRepo.AddAsync(productBrand);
                    await _UnitOfWork.Commit();
                    return ServiceResult<ProductBrand>.Success(productBrand);
                }
                else
                {
                    // Configuration error: Repository type mismatch
                    return ServiceResult<ProductBrand>.Failure(
                        "Internal configuration error: Product repository type mismatch.",
                        ServiceErrorType.RepositoryTypeMismatch
                    );
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating product brand: {ex}");

                return ServiceResult<ProductBrand>.Failure(
                    "An unexpected internal error occurred during product property creation.",
                    ServiceErrorType.InternalError
                );
            }
        }
    }
}
