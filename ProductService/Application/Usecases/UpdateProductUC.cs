using ProductService.Application.UnitOfWork;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.UnitOfWork;
using ProductService.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Application.Usecases
{
    public class UpdateProductUC
    {
        private readonly IUnitOfWork unitOfWork;
        public UpdateProductUC(ProductContext productContext)
        {
            this.unitOfWork = new ProductUnitOfWork(productContext);
        }

        public async Task<Product?> UpdateProduct(int productID, Product newProduct)
        {
            try
            {
                IQueryable<Product> productQuery = this.unitOfWork.ProductRepository().GetByIdQueryable(productID);
                Product? p = await productQuery.FirstOrDefaultAsync();
                if (p == null) return null;

                p.Description = newProduct.Description;
                p.Price = newProduct.Price;
                p.Quantity = newProduct.Quantity;
                p.Status = newProduct.Status;
                p.Name = newProduct.Name;

                
                await this.unitOfWork.Commit();
                return p;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật Product" + ex.ToString());
                return null;
            }
        }

        public async Task<ProductProperty?> UpdateProductProperty(int productPropertyID, ProductProperty newproductProperty)
        {
            try
            {
                IQueryable<ProductProperty> ProductPropertyQuery = this.unitOfWork.ProductPropertyRepository().GetByIdQueryable(productPropertyID);
                ProductProperty? productProperty = await ProductPropertyQuery.FirstOrDefaultAsync();
                if (productProperty == null) return null;

                productProperty.Description = newproductProperty.Description;
                productProperty.Name = newproductProperty.Name;
                await this.unitOfWork.Commit();
                return productProperty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật ProductProperty" + ex.ToString());
                return null;
            }
        }
    }
}
