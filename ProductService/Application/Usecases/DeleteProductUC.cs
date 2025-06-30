using ApiDto.Response;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.UnitOfWork;

namespace ProductService.Application.Usecases
{
    public class DeleteProductUC
    {
        private readonly IUnitOfWork unitOfWork;
        public DeleteProductUC(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<DeletionResult<ProductPropertyDetail>> DeleteProductPropertyDetail(ProductPropertyDetail productPropertyDetail)
        {
            try
            {
                if (productPropertyDetail == null)
                {
                    return DeletionResult<ProductPropertyDetail>.Failure(
                        "ProductPropertyDetail cannot be null.",
                        DeletionErrorType.ValidationError
                    );
                }

                IQueryable<ProductPropertyDetail> query = this.unitOfWork.ProductPropertyDetailRepository().
                    GetByCompositeKey(productPropertyDetail.ProductID, "ProductID",
                    productPropertyDetail.ProductPropertyID, "ProductPropertyID");

                ProductPropertyDetail? existingDetail = await query.FirstOrDefaultAsync();
                if (existingDetail == null)
                {
                    return DeletionResult<ProductPropertyDetail>.Failure(
                        $"ProductPropertyDetail not found.",
                        DeletionErrorType.NotFound
                    );
                }

                this.unitOfWork.ProductPropertyDetailRepository().Remove(productPropertyDetail);
                await this.unitOfWork.Commit();

                return DeletionResult<ProductPropertyDetail>.Success(productPropertyDetail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa ProductPropertyDetail: {ex.Message}");
                return DeletionResult<ProductPropertyDetail>.Failure(
                    "An internal error occurred during deletion.",
                    DeletionErrorType.InternalError
                );
            }
        }

        public async Task<DeletionResult<Product>> DeleteProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    return DeletionResult<Product>.Failure(
                        "Product cannot be null.",
                        DeletionErrorType.ValidationError
                    );
                }

                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetByIdQueryable(product.ID);


                Product? existingProduct = await query.FirstOrDefaultAsync();
                if (existingProduct == null)
                {
                    return DeletionResult<Product>.Failure(
                        $"ProductPropertyDetail not found.",
                        DeletionErrorType.NotFound
                    );
                }

                this.unitOfWork.ProductRepository().Remove(existingProduct);
                await this.unitOfWork.Commit();

                return DeletionResult<Product>.Success(existingProduct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa Product: {ex.Message}");
                return DeletionResult<Product>.Failure(
                    "An internal error occurred during deletion.",
                    DeletionErrorType.InternalError
                );
            }
        }

        public async Task<DeletionResult<ProductProperty>> DeleteProductProperty(ProductProperty productProperty)
        {
            try
            {
                if (productProperty == null)
                {
                    return DeletionResult<ProductProperty>.Failure(
                        "ProductProperty cannot be null.",
                        DeletionErrorType.ValidationError
                    );
                }

                IQueryable<ProductProperty> query = this.unitOfWork.ProductPropertyRepository().GetByIdQueryable(productProperty.ID);
                    

                ProductProperty? existingProductProperty = await query.FirstOrDefaultAsync();
                if (existingProductProperty == null)
                {
                    return DeletionResult<ProductProperty>.Failure(
                        $"ProductProperty not found.",
                        DeletionErrorType.NotFound
                    );
                }

                this.unitOfWork.ProductPropertyRepository().Remove(productProperty);
                await this.unitOfWork.Commit();

                return DeletionResult<ProductProperty>.Success(productProperty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa Product Property: {ex.Message}");
                return DeletionResult<ProductProperty>.Failure(
                    "An internal error occurred during deletion.",
                    DeletionErrorType.InternalError
                );
            }
        }


        public async Task<DeletionResult<ProductPropertyDetail>> DeleteProductPropertyDetails(List<ProductPropertyDetail> productPropertyDetails)
        {
            try
            {
                if (productPropertyDetails == null || !productPropertyDetails.Any())
                {
                    return DeletionResult<ProductPropertyDetail>.Failure(
                        "No product property detail IDs provided to delete.",
                        DeletionErrorType.ValidationError
                    );
                }

    
                  List<ProductPropertyDetail>? deletedDetails = await this.unitOfWork
                                                                      .ProductPropertyDetailRepository()
                                                                      .RemoveRange(productPropertyDetails); 


                if (deletedDetails == null || !deletedDetails.Any())
                {
                    return DeletionResult<ProductPropertyDetail>.Failure(
                       "Product property details not found or already deleted.",
                       DeletionErrorType.NotFound
                   );
                }

                await this.unitOfWork.Commit();

                return DeletionResult<ProductPropertyDetail>.Success(deletedDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa Product Property Detail: {ex.Message}");
                return DeletionResult<ProductPropertyDetail>.Failure(
                    "An internal error occurred during deletion.",
                    DeletionErrorType.InternalError
                );
            }
        }


        public async Task<DeletionResult<ProductType>> DeleteProductType(ProductType productType)
        {
            try
            {
                if (productType == null)
                {
                    return DeletionResult<ProductType>.Failure(
                        "Product type cannot be null.",
                        DeletionErrorType.ValidationError
                    );
                }

                IQueryable<ProductType> query = this.unitOfWork.ProductTypeRepository().GetByIdQueryable(productType.ID);


                ProductType? existingProductType = await query.FirstOrDefaultAsync();
                if (existingProductType == null)
                {
                    return DeletionResult<ProductType>.Failure(
                        $"Product type not found.",
                        DeletionErrorType.NotFound
                    );
                }

                this.unitOfWork.ProductTypeRepository().Remove(existingProductType);
                await this.unitOfWork.Commit();

                return DeletionResult<ProductType>.Success(existingProductType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa Product type: {ex.Message}");
                return DeletionResult<ProductType>.Failure(
                    "An internal error occurred during deletion.",
                    DeletionErrorType.InternalError
                );
            }
        }

        public async Task<DeletionResult<ProductBrand>> DeleteProductBrand(ProductBrand productBrand)
        {
            try
            {
                if (productBrand == null)
                {
                    return DeletionResult<ProductBrand>.Failure(
                        "Product brand cannot be null.",
                        DeletionErrorType.ValidationError
                    );
                }

                IQueryable<ProductBrand> query = this.unitOfWork.ProductBrandRepository().GetByIdQueryable(productBrand.ID);


                ProductBrand? existingProductBrand = await query.FirstOrDefaultAsync();
                if (existingProductBrand == null)
                {
                    return DeletionResult<ProductBrand>.Failure(
                        $"Product brand not found.",
                        DeletionErrorType.NotFound
                    );
                }

                this.unitOfWork.ProductBrandRepository().Remove(existingProductBrand);
                await this.unitOfWork.Commit();

                return DeletionResult<ProductBrand>.Success(existingProductBrand);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa Product brand: {ex.Message}");
                return DeletionResult<ProductBrand>.Failure(
                    "An internal error occurred during deletion.",
                    DeletionErrorType.InternalError
                );
            }
        }
    }
}
