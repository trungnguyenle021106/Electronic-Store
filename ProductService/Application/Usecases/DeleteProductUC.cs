using CommonDto.ResultDTO;
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

        public async Task<ServiceResult<ProductPropertyDetail>> DeleteProductPropertyDetail(ProductPropertyDetail productPropertyDetail)
        {
            try
            {
                if (productPropertyDetail == null)
                {
                    return ServiceResult<ProductPropertyDetail>.Failure(
                        "ProductPropertyDetail cannot be null.",
                        ServiceErrorType.ValidationError
                    );
                }

                IQueryable<ProductPropertyDetail> query = this.unitOfWork.ProductPropertyDetailRepository().
                    GetByCompositeKey(productPropertyDetail.ProductID, "ProductID",
                    productPropertyDetail.ProductPropertyID, "ProductPropertyID");

                ProductPropertyDetail? existingDetail = await query.FirstOrDefaultAsync();
                if (existingDetail == null)
                {
                    return ServiceResult<ProductPropertyDetail>.Failure(
                        $"ProductPropertyDetail not found.",
                        ServiceErrorType.NotFound
                    );
                }

                this.unitOfWork.ProductPropertyDetailRepository().Remove(productPropertyDetail);
                await this.unitOfWork.Commit();

                return ServiceResult<ProductPropertyDetail>.Success(productPropertyDetail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa ProductPropertyDetail: {ex.Message}");
                return ServiceResult<ProductPropertyDetail>.Failure(
                    "An internal error occurred during deletion.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<Product>> DeleteProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    return ServiceResult<Product>.Failure(
                        "Product cannot be null.",
                        ServiceErrorType.ValidationError
                    );
                }

                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetByIdQueryable(product.ID);

                Product? existingProduct = await query.FirstOrDefaultAsync();
                if (existingProduct == null)
                {
                    return ServiceResult<Product>.Failure(
                        $"ProductPropertyDetail not found.",
                        ServiceErrorType.NotFound
                    );
                }

                this.unitOfWork.ProductRepository().Remove(existingProduct);
                await this.unitOfWork.Commit();

                return ServiceResult<Product>.Success(existingProduct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa Product: {ex.Message}");
                return ServiceResult<Product>.Failure(
                    "An internal error occurred during deletion.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<ProductProperty>> DeleteProductProperty(int id)
        {
            if (id <= 0)
            {
                return ServiceResult<ProductProperty>.Failure(
                    "ProductPropertyID not valid.",
                    ServiceErrorType.ValidationError
                );
            }
            try
            {
                IQueryable<ProductProperty> query = this.unitOfWork.ProductPropertyRepository().GetByIdQueryable(id);

                ProductProperty? existingProductProperty = await query.FirstOrDefaultAsync();
                if (existingProductProperty == null)
                {
                    return ServiceResult<ProductProperty>.Failure(
                        $"ProductProperty not found.",
                        ServiceErrorType.NotFound
                    );
                }

                this.unitOfWork.ProductPropertyRepository().Remove(existingProductProperty);
                await this.unitOfWork.Commit();

                return ServiceResult<ProductProperty>.Success(existingProductProperty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa Product Property: {ex.Message}");
                return ServiceResult<ProductProperty>.Failure(
                    "An internal error occurred during deletion.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<ProductPropertyDetail>> DeleteProductPropertyDetails(List<ProductPropertyDetail> productPropertyDetails)
        {
            try
            {
                if (productPropertyDetails == null || !productPropertyDetails.Any())
                {
                    return ServiceResult<ProductPropertyDetail>.Failure(
                        "No product property detail IDs provided to delete.",
                        ServiceErrorType.ValidationError
                    );
                }

                List<ProductPropertyDetail>? deletedDetails = await this.unitOfWork
                                                                      .ProductPropertyDetailRepository()
                                                                      .RemoveRange(productPropertyDetails);

                if (deletedDetails == null || !deletedDetails.Any())
                {
                    return ServiceResult<ProductPropertyDetail>.Failure(
                       "Product property details not found or already deleted.",
                       ServiceErrorType.NotFound
                   );
                }

                await this.unitOfWork.Commit();

                return ServiceResult<ProductPropertyDetail>.Success(deletedDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa Product Property Detail: {ex.Message}");
                return ServiceResult<ProductPropertyDetail>.Failure(
                    "An internal error occurred during deletion.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<ProductType>> DeleteProductType(int ID)
        {
            if (ID <= 0)
            {
                return ServiceResult<ProductType>.Failure(
                    "ProductTypeID not valid.",
                    ServiceErrorType.ValidationError
                );
            }
            try
            {
                ProductType? existingProductType = await this.unitOfWork.ProductTypeRepository().GetById(ID);
                if (existingProductType == null)
                {
                    return ServiceResult<ProductType>.Failure(
                        $"Product type not found.",
                        ServiceErrorType.NotFound
                    );
                }

                this.unitOfWork.ProductTypeRepository().Remove(existingProductType);
                await this.unitOfWork.Commit();

                return ServiceResult<ProductType>.Success(existingProductType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa Product type: {ex.Message}");
                return ServiceResult<ProductType>.Failure(
                    "An internal error occurred during deletion.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<ProductBrand>> DeleteProductBrand(int ID)
        {
            if (ID <= 0)
            {
                return ServiceResult<ProductBrand>.Failure(
                    "ProductBrandeID not valid.",
                    ServiceErrorType.ValidationError
                );
            }
            try
            {
                ProductBrand? existingProductBrand = await this.unitOfWork.ProductBrandRepository().GetById(ID);

                if (existingProductBrand == null)
                {
                    return ServiceResult<ProductBrand>.Failure(
                        $"Product brand not found.",
                        ServiceErrorType.NotFound
                    );
                }

                this.unitOfWork.ProductBrandRepository().Remove(existingProductBrand);
                await this.unitOfWork.Commit();

                return ServiceResult<ProductBrand>.Success(existingProductBrand);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xóa Product brand: {ex.Message}");
                return ServiceResult<ProductBrand>.Failure(
                    "An internal error occurred during deletion.",
                    ServiceErrorType.InternalError
                );
            }
        }
    }
}
