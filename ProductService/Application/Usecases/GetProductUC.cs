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
                return await this.unitOfWork.ProductRepository().GetById(id);
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
                return (List<Product>) await  this.unitOfWork.ProductRepository().GetAll();
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
                return (List<ProductProperty>)await this.unitOfWork.ProductPropertyRepository().GetAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách thuộc tính sản phẩm: {ex.Message}");
                return null;
            }
        }
    }
}
