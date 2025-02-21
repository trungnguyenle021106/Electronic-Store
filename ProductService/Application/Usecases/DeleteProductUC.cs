using ProductService.Application.UnitOfWork;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.UnitOfWork;
using ProductService.Infrastructure.DBContext;

namespace ProductService.Application.Usecases
{
    public class DeleteProductUC
    {
        private readonly IUnitOfWork unitOfWork;
        public DeleteProductUC(ProductContext productContext)
        {
            this.unitOfWork = new ProductUnitOfWork(productContext);
        }

        public async Task<bool> DeletePropertyDetail(ProductPropertyDetail productPropertyDetail)
        {
            try
            {
                this.unitOfWork.ProductPropertyDetailRepository().Delete(productPropertyDetail);
                await this.unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi xóa ProductPropertyDetail" + ex.ToString());
                return false;
            }
        }
    }
}
