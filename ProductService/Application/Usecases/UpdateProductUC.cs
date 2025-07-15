using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.UnitOfWork;

namespace ProductService.Application.Usecases
{
    public class UpdateProductUC
    {
        private readonly IUnitOfWork unitOfWork;
        public UpdateProductUC(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<Product>> UpdateProduct(int productID, Product newProduct)
        {
            try
            {
                IQueryable<Product> productQuery = this.unitOfWork.ProductRepository().GetByIdQueryable(productID);
                Product? existingProduct = await productQuery.FirstOrDefaultAsync();
                if (existingProduct == null)
                {
                    return ServiceResult<Product>.Failure("Product not found.", ServiceErrorType.NotFound);
                }

                existingProduct.Description = newProduct.Description;
                existingProduct.Price = newProduct.Price;
                existingProduct.Quantity = newProduct.Quantity;
                existingProduct.Status = newProduct.Status;
                existingProduct.Name = newProduct.Name;
                existingProduct.ProductTypeID = newProduct.ProductTypeID;
                existingProduct.ProductBrandID = newProduct.ProductBrandID;

                await this.unitOfWork.Commit();
                return ServiceResult<Product>.Success(existingProduct);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Product: " + ex.ToString());
                return ServiceResult<Product>.Failure("An internal error occurred during product update.", ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<ProductProperty>> UpdateProductProperty(int productPropertyID, ProductProperty newproductProperty)
        {
            try
            {
                ProductProperty? productProperty = await this.unitOfWork.ProductPropertyRepository().GetById(productPropertyID);
 
                if (productProperty == null)
                {
                    return ServiceResult<ProductProperty>.Failure("Product property not found.", ServiceErrorType.NotFound);
                }
                if (!productProperty.Description.Equals(""))
                    productProperty.Description = newproductProperty.Description;
                if (!productProperty.Name.Equals(""))
                    productProperty.Name = newproductProperty.Name;

                await this.unitOfWork.Commit();
                return ServiceResult<ProductProperty>.Success(productProperty);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Product Property: " + ex.ToString());
                return ServiceResult<ProductProperty>.Failure("An internal error occurred during product property update.", ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<ProductType>> UpdateProductType(int productTypeID, ProductType newProductType)
        {
            try
            {
                IQueryable<ProductType> query = this.unitOfWork.ProductTypeRepository().GetByIdQueryable(productTypeID);
                ProductType? existingProductType = await query.FirstOrDefaultAsync();
                if (existingProductType == null)
                {
                    return ServiceResult<ProductType>.Failure("Product type not found.", ServiceErrorType.NotFound);
                }
                existingProductType.Name = newProductType.Name;

                await this.unitOfWork.Commit();
                return ServiceResult<ProductType>.Success(existingProductType);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Product type: " + ex.ToString());
                return ServiceResult<ProductType>.Failure("An internal error occurred during product update.", ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<ProductBrand>> UpdateProductBrand(int productBrandID, ProductBrand newProductBrand)
        {
            try
            {
                IQueryable<ProductBrand> query = this.unitOfWork.ProductBrandRepository().GetByIdQueryable(productBrandID);
                ProductBrand? existingProductBrand = await query.FirstOrDefaultAsync();
                if (existingProductBrand == null)
                {
                    return ServiceResult<ProductBrand>.Failure("Product brand not found.", ServiceErrorType.NotFound);
                }
                existingProductBrand.Name = newProductBrand.Name;

                await this.unitOfWork.Commit();
                return ServiceResult<ProductBrand>.Success(existingProductBrand);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Product brand: " + ex.ToString());
                return ServiceResult<ProductBrand>.Failure("An internal error occurred during product update.", ServiceErrorType.InternalError);
            }
        }
    }
}
