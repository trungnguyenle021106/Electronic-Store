using Microsoft.EntityFrameworkCore;
using ProductService.Application.UnitOfWork;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.UnitOfWork;
using ProductService.Infrastructure.DBContext;
using System.Collections.Generic;

namespace ProductService.Application.Usecases
{
    public class GetProductUC
    {
        private readonly IUnitOfWork unitOfWork;
        public GetProductUC(ProductContext productContext)
        {
            this.unitOfWork = new ProductUnitOfWork(productContext);
        }

        public async Task<Product?> GetProductByID(int id)
        {
            try
            {
                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetByIdQueryable(id);
                Product? product = await query.FirstOrDefaultAsync();
                if(product == null) {return null;}

                return product;
            }
            catch (Exception ex) {
                Console.WriteLine($"Lỗi lấy sản phẩm có id : {id}, lỗi : {ex.Message}");
                return null;    
            }
        }

        public async Task<List<Product>?> GetAllProducts()
        {
            try
            {
                IQueryable<Product> productsQuery = this.unitOfWork.ProductRepository().GetAll();
                List<Product> productsList = await productsQuery.ToListAsync().ConfigureAwait(false);
                return productsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách sản phẩm : {ex.Message}");
                return null;
            }
        }

        public async Task<List<ProductProperty>?> GetAllProductProperties()
        {
            try
            {
                IQueryable<ProductProperty> productPropertiesQuery = this.unitOfWork.ProductPropertyRepository().GetAll();
                List<ProductProperty> productPropertiesList = await productPropertiesQuery.ToListAsync().ConfigureAwait(false);
                return productPropertiesList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách thuộc tính sản phẩm: {ex.Message}");
                return null;
            }
        }
    }
}
