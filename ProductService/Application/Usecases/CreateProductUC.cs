using ProductService.Application.UnitOfWork;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.UnitOfWork;
using ProductService.Infrastructure.DBContext;

namespace ProductService.Application.Usecases
{
    public class CreateProductUC
    {
        private readonly IUnitOfWork _UnitOfWork;
        public CreateProductUC(ProductContext productContext)
        {
            _UnitOfWork = new ProductUnitOfWork(productContext);
        }

        public async Task<Product?> CreateProduct(Product product)
        {
            try
            {
                Product newProduct = await this._UnitOfWork.ProductRepository().Add(product);
                await this._UnitOfWork.Commit();
                return newProduct;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi tạo Product :" + ex.ToString());
                return null;
            }
        }

        public async Task<bool> AddPropertiesToProduct(int ProductID, List<int> listProductPropertyIDs)
        {
            try
            {
                foreach (int ProductPropertyID in listProductPropertyIDs) {
                    await this._UnitOfWork.ProductPropertyDetailRepository().Add(new ProductPropertyDetail
                    {
                        ProductID = ProductID,
                        ProductPropertyID = ProductPropertyID
                    });
                }
                await this._UnitOfWork.Commit();
                return true;
            }
            catch (Exception ex) {
                Console.WriteLine("Lỗi thêm thuộc tính cho Product :" + ex.ToString());
                return false;
            }
        }

        public async Task<ProductProperty?> CreateProductProperty(ProductProperty productProperty)
        {
            try
            {
                ProductProperty newProductProperty = await this._UnitOfWork.ProductPropertyRepository().Add(productProperty);
                await this._UnitOfWork.Commit();
                return newProductProperty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi tạo Product Type :" + ex.ToString());
                return null;
            }
        }
    }
}
