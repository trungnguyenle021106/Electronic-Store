
using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.UnitOfWork;
using ProductService.Infrastructure.Data.Repositories;
using ProductService.Infrastructure.DTO;
using System.Linq;

namespace ProductService.Application.Usecases
{
    public class UpdateProductUC
    {

        private readonly IUnitOfWork _UnitOfWork;
        private readonly ManageProductImagesUC manageProductImagesUC;
        public UpdateProductUC(IUnitOfWork unitOfWork, ManageProductImagesUC manageProductImagesUC)
        {
            this._UnitOfWork = unitOfWork;
            this.manageProductImagesUC = manageProductImagesUC;
        }

        public async Task<ServiceResult<Product>> UpdateProductsQuantity(List<OrderProduct> productsToUpdate)
        {
            if (productsToUpdate == null || !productsToUpdate.Any())
            {
                return ServiceResult<Product>.Failure("No product data provided for update or list is empty.", ServiceErrorType.ValidationError);
            }

            var deficientProducts = new List<Product>();

            try
            {
                var productIdsToUpdate = productsToUpdate.Select(p => p.ProductID).ToList();


                List<Product> existingProductsInDb = await _UnitOfWork.ProductRepository().GetAll()
                                                                      .Where(p => productIdsToUpdate.Contains(p.ID))
                                                                      .ToListAsync();

                foreach (OrderProduct orderProduct in productsToUpdate)
                {
                    Product? existingProduct = existingProductsInDb.FirstOrDefault(p => p.ID == orderProduct.ProductID);

                    if (existingProduct != null)
                    {
                        int originalQuantity = existingProduct.Quantity;
                        int adjustmentQuantity = orderProduct.Quantity;
                        int newCalculatedQuantity = originalQuantity + adjustmentQuantity;


                        if (newCalculatedQuantity < 0)
                        {
                            deficientProducts.Add(new Product
                            {
                                ID = existingProduct.ID,
                                Name = existingProduct.Name,
                                Quantity = newCalculatedQuantity
                            });

                            existingProduct.Quantity = newCalculatedQuantity;
                            existingProduct.Status = "Hết hàng";
                        }
                        else
                        {
                            existingProduct.Quantity = newCalculatedQuantity;
                            existingProduct.Status = "Còn hàng";
                        }
                    }
                }

                await _UnitOfWork.Commit();
                return ServiceResult<Product>.Success(deficientProducts);
            }
            catch (Exception ex)
            {
                return ServiceResult<Product>.Failure($"An error occurred while updating product quantity: {ex.Message}", ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Product>> UpdateProduct(Product product, List<int> productPropertyIDs, IFormFile file)
        {
            if (product == null)
            {
                return ServiceResult<Product>.Failure("No product data provided for update.", ServiceErrorType.ValidationError);
            }
            if (productPropertyIDs == null || !productPropertyIDs.Any())
            {
                return ServiceResult<Product>.Failure("No product property IDs provided for update.", ServiceErrorType.ValidationError);
            }

            try
            {
                Product? existingProduct = await this._UnitOfWork.ProductRepository().GetById(product.ID);

                if (existingProduct == null)
                {
                    return ServiceResult<Product>.Failure($"Product with ID '{product.ID}' not found. Update failed.", ServiceErrorType.NotFound);
                }

                using (var transaction = await _UnitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        existingProduct.Name = product.Name;
                        existingProduct.Description = product.Description;
                        existingProduct.Price = product.Price;
                        existingProduct.Quantity = product.Quantity;
                        existingProduct.Status = product.Status;
                        existingProduct.ProductBrandID = product.ProductBrandID;
                        existingProduct.ProductTypeID = product.ProductTypeID;
                        IQueryable<ProductPropertyDetail> proPropQuery = this._UnitOfWork
                            .ProductPropertyDetailRepository()
                            .GetEntitiesByForeignKeyId(product.ID, "ProductID");

                        List<ProductPropertyDetail> existingProperties = await proPropQuery
                          .ToListAsync()
                          .ConfigureAwait(false);

                        List<ProductPropertyDetail> productPropertyDetailsToRemove = new List<ProductPropertyDetail>();
                        foreach (ProductPropertyDetail property in existingProperties)
                        {
                            if (!productPropertyIDs.Contains(property.ProductPropertyID))
                            {
                                productPropertyDetailsToRemove.Add(property);
                            }
                        }
                        if (productPropertyDetailsToRemove.Any())
                        {
                            await this._UnitOfWork.ProductPropertyDetailRepository().RemoveRange(productPropertyDetailsToRemove);
                            await this._UnitOfWork.Commit();
                        }

                        List<int> existingPropertyIds = await proPropQuery
                        .Select(ppd => ppd.ProductPropertyID)
                        .ToListAsync()
                        .ConfigureAwait(false);
                        HashSet<int> existingPropertyIdsSet = new HashSet<int>(existingPropertyIds);
                        List<int> newProductPropertyIDsToAdd = new List<int>();

                        foreach (int newId in productPropertyIDs.Distinct())
                        {
                            if (!existingPropertyIdsSet.Contains(newId))
                            {
                                newProductPropertyIDsToAdd.Add(newId);
                            }
                        }

                        if (newProductPropertyIDsToAdd.Any())
                        {
                            var entitiesToAdd = newProductPropertyIDsToAdd.Select(propId => new ProductPropertyDetail
                            {
                                ProductID = product.ID,
                                ProductPropertyID = propId
                            }).ToList();

                            await this._UnitOfWork.ProductPropertyDetailRepository().AddRangeAsync(entitiesToAdd);
                            await _UnitOfWork.Commit();
                        }


                        if (file != null)
                        {
                            string imageName = Path.GetFileName(product.Image);

                            string imageUrl = await manageProductImagesUC.UploadImageAsync(
                                file.OpenReadStream(),
                                imageName,
                                file.ContentType,
                                "product_images/",
                                true
                            );

                            if (imageUrl == null)
                            {
                                await _UnitOfWork.RollbackAsync(transaction);
                                return ServiceResult<Product>.Failure("Product updated but image upload failed. Please try uploading the image again.", ServiceErrorType.InternalError);
                            }
                        }

                        await _UnitOfWork.Commit();
                        await _UnitOfWork.CommitTransactionAsync(transaction);
                        return ServiceResult<Product>.Success(product);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error updating product during transaction: {ex}");
                        await _UnitOfWork.RollbackAsync(transaction);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error updating product: {ex}");
                return ServiceResult<Product>.Failure(
                    "An unexpected internal error occurred during product update.",
                    ServiceErrorType.InternalError
                );
            }
        }

        public async Task<ServiceResult<ProductProperty>> UpdateProductProperty(int productPropertyID, ProductProperty newproductProperty)
        {
            try
            {
                ProductProperty? productProperty = await this._UnitOfWork.ProductPropertyRepository().GetById(productPropertyID);

                if (productProperty == null)
                {
                    return ServiceResult<ProductProperty>.Failure("Product property not found.", ServiceErrorType.NotFound);
                }
                if (!productProperty.Description.Equals(""))
                    productProperty.Description = newproductProperty.Description;
                if (!productProperty.Name.Equals(""))
                    productProperty.Name = newproductProperty.Name;

                await this._UnitOfWork.Commit();
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
                IQueryable<ProductType> query = this._UnitOfWork.ProductTypeRepository().GetByIdQueryable(productTypeID);
                ProductType? existingProductType = await query.FirstOrDefaultAsync();
                if (existingProductType == null)
                {
                    return ServiceResult<ProductType>.Failure("Product type not found.", ServiceErrorType.NotFound);
                }
                existingProductType.Name = newProductType.Name;

                await this._UnitOfWork.Commit();
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
                IQueryable<ProductBrand> query = this._UnitOfWork.ProductBrandRepository().GetByIdQueryable(productBrandID);
                ProductBrand? existingProductBrand = await query.FirstOrDefaultAsync();
                if (existingProductBrand == null)
                {
                    return ServiceResult<ProductBrand>.Failure("Product brand not found.", ServiceErrorType.NotFound);
                }
                existingProductBrand.Name = newProductBrand.Name;

                await this._UnitOfWork.Commit();
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
