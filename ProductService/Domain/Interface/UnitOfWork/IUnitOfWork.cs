using Microsoft.EntityFrameworkCore.Storage;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.IRepositories;

namespace ProductService.Domain.Interface.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Product> ProductRepository();
        IRepository<ProductType> ProductTypeRepository();
        IRepository<ProductBrand> ProductBrandRepository();
        IRepository<ProductProperty> ProductPropertyRepository();
        IRepository<ProductPropertyDetail> ProductPropertyDetailRepository();

        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync(IDbContextTransaction transaction);
        Task RollbackAsync(IDbContextTransaction transaction);
        Task Commit();

    }
}
