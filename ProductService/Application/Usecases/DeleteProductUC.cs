
using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.UnitOfWork;

namespace ProductService.Application.Usecases
{
    public class DeleteProductUC
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ManageProductImagesUC manageProductImagesUC;
        public DeleteProductUC(IUnitOfWork unitOfWork, ManageProductImagesUC manageProductImagesUC)
        {
            this._UnitOfWork = unitOfWork;
            this.manageProductImagesUC = manageProductImagesUC;
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

                IQueryable<ProductPropertyDetail> query = this._UnitOfWork.ProductPropertyDetailRepository().
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

                this._UnitOfWork.ProductPropertyDetailRepository().Remove(productPropertyDetail);
                await this._UnitOfWork.Commit();

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

        public async Task<ServiceResult<Product>> DeleteProduct(int id)

        {
            if (id <= 0)
            {
                return ServiceResult<Product>.Failure(
                    "ProductyID not valid.",
                    ServiceErrorType.ValidationError
                );
            }
            try
            {
                Product? existingProduct = await this._UnitOfWork.ProductRepository().GetById(id);
                if (existingProduct == null)
                {
                    return ServiceResult<Product>.Failure(
                        $"ProductPropertyDetail not found.",
                        ServiceErrorType.NotFound
                    );
                }
                using (var transaction = await _UnitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        this._UnitOfWork.ProductRepository().Remove(existingProduct);
                        await this._UnitOfWork.Commit();

                        Uri uri = new Uri(existingProduct.Image);
                        string pathAndQuery = uri.PathAndQuery;
                        string s3Key = pathAndQuery.TrimStart('/');
                        await this.manageProductImagesUC.DeleteImageAsync(s3Key);

                        await _UnitOfWork.CommitTransactionAsync(transaction);
                        return ServiceResult<Product>.Success(existingProduct);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error delete product during transaction: {ex}");
                        await _UnitOfWork.RollbackAsync(transaction); // Rollback toàn bộ nếu có lỗi trong DB transaction
                        throw; // Ném lại lỗi để catch bên ngoài xử lý
                    }
                }
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
                IQueryable<ProductProperty> query = this._UnitOfWork.ProductPropertyRepository().GetByIdQueryable(id);

                ProductProperty? existingProductProperty = await query.FirstOrDefaultAsync();
                if (existingProductProperty == null)
                {
                    return ServiceResult<ProductProperty>.Failure(
                        $"ProductProperty not found.",
                        ServiceErrorType.NotFound
                    );
                }

                this._UnitOfWork.ProductPropertyRepository().Remove(existingProductProperty);
                await this._UnitOfWork.Commit();

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
                await this._UnitOfWork.ProductPropertyDetailRepository().RemoveRange(productPropertyDetails);
                List<ProductPropertyDetail>? deletedDetails = productPropertyDetails;

                if (deletedDetails == null || !deletedDetails.Any())
                {
                    return ServiceResult<ProductPropertyDetail>.Failure(
                       "Product property details not found or already deleted.",
                       ServiceErrorType.NotFound
                   );
                }

                await this._UnitOfWork.Commit();

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
                ProductType? existingProductType = await this._UnitOfWork.ProductTypeRepository().GetById(ID);
                if (existingProductType == null)
                {
                    return ServiceResult<ProductType>.Failure(
                        $"Product type not found.",
                        ServiceErrorType.NotFound
                    );
                }

                this._UnitOfWork.ProductTypeRepository().Remove(existingProductType);
                await this._UnitOfWork.Commit();

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
                ProductBrand? existingProductBrand = await this._UnitOfWork.ProductBrandRepository().GetById(ID);

                if (existingProductBrand == null)
                {
                    return ServiceResult<ProductBrand>.Failure(
                        $"Product brand not found.",
                        ServiceErrorType.NotFound
                    );
                }

                this._UnitOfWork.ProductBrandRepository().Remove(existingProductBrand);
                await this._UnitOfWork.Commit();

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
