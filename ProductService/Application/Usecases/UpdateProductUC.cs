using ApiDto.Response;
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

        public async Task<UpdateResult<Product>> UpdateProduct(int productID, Product newProduct)
        {
            try
            {
                IQueryable<Product> productQuery = this.unitOfWork.ProductRepository().GetByIdQueryable(productID);
                Product? existingProduct = await productQuery.FirstOrDefaultAsync();
                if (existingProduct == null)
                {
                    return UpdateResult<Product>.Failure("Product not found.", UpdateErrorType.NotFound);
                }

                existingProduct.Description = newProduct.Description;
                existingProduct.Price = newProduct.Price;
                existingProduct.Quantity = newProduct.Quantity;
                existingProduct.Status = newProduct.Status;
                existingProduct.Name = newProduct.Name;
                existingProduct.ProductTypeID = newProduct.ProductTypeID;
                existingProduct.ProductBrandID = newProduct.ProductBrandID;

                await this.unitOfWork.Commit();
                return UpdateResult<Product>.Success(existingProduct);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Product: " + ex.ToString());
                return UpdateResult<Product>.Failure("An internal error occurred during product update.", UpdateErrorType.InternalError);
            }
        }

        public async Task<UpdateResult<ProductProperty>> UpdateProductProperty(int productPropertyID, ProductProperty newproductProperty)
        {
            try
            {
                IQueryable<ProductProperty> ProductPropertyQuery = this.unitOfWork.ProductPropertyRepository().GetByIdQueryable(productPropertyID);
                ProductProperty? productProperty = await ProductPropertyQuery.FirstOrDefaultAsync();
                if (productProperty == null)
                {
                    return UpdateResult<ProductProperty>.Failure("Product property not found.", UpdateErrorType.NotFound);
                }

                productProperty.Description = newproductProperty.Description;
                productProperty.Name = newproductProperty.Name;

                await this.unitOfWork.Commit();
                return UpdateResult<ProductProperty>.Success(productProperty);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Product Property: " + ex.ToString());
                return UpdateResult<ProductProperty>.Failure("An internal error occurred during product property update.", UpdateErrorType.InternalError);
            }
        }

        public async Task<UpdateResult<ProductType>> UpdateProductType(int productTypeID, ProductType newProductType)
        {
            try
            {
                IQueryable<ProductType> query = this.unitOfWork.ProductTypeRepository().GetByIdQueryable(productTypeID);
                ProductType? existingProductType = await query.FirstOrDefaultAsync();
                if (existingProductType == null)
                {
                    return UpdateResult<ProductType>.Failure("Product type not found.", UpdateErrorType.NotFound);
                }
                existingProductType.Name = newProductType.Name;

                await this.unitOfWork.Commit();
                return UpdateResult<ProductType>.Success(existingProductType);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Product type: " + ex.ToString());
                return UpdateResult<ProductType>.Failure("An internal error occurred during product update.", UpdateErrorType.InternalError);
            }
        }

        public async Task<UpdateResult<ProductBrand>> UpdateProductBrand(int productBrandID, ProductBrand newProductBrand)
        {
            try
            {
                IQueryable<ProductBrand> query = this.unitOfWork.ProductBrandRepository().GetByIdQueryable(productBrandID);
                ProductBrand? existingProductBrand = await query.FirstOrDefaultAsync();
                if (existingProductBrand == null)
                {
                    return UpdateResult<ProductBrand>.Failure("Product brand not found.", UpdateErrorType.NotFound);
                }
                existingProductBrand.Name = newProductBrand.Name;

                await this.unitOfWork.Commit();
                return UpdateResult<ProductBrand>.Success(existingProductBrand);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Product brand: " + ex.ToString());
                return UpdateResult<ProductBrand>.Failure("An internal error occurred during product update.", UpdateErrorType.InternalError);
            }
        }
    }
}
