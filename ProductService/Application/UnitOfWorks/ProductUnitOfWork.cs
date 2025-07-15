
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.IRepositories;
using ProductService.Domain.Interface.UnitOfWork;
using ProductService.Infrastructure.Data.DBContext;
using ProductService.Infrastructure.Data.Repositories;


namespace ProductService.Application.UnitOfWork
{
    public class ProductUnitOfWork : IUnitOfWork
    {
        private readonly ProductContext _Context;
        private readonly IRepository<Product> _ProductRepository;
        private readonly IRepository<ProductProperty> _ProductPropertyRepository;
        private readonly IRepository<ProductPropertyDetail> _ProductPropertyDetailRepository;
        private readonly IRepository<ProductBrand> _ProductBrandRepository;
        private readonly IRepository<ProductType> _ProductTypeRepository;

        public ProductUnitOfWork(ProductContext context)
        {
            this._ProductRepository = new Repository<Product>(context);
            this._ProductPropertyRepository = new Repository<ProductProperty>(context);
            this._ProductPropertyDetailRepository = new Repository<ProductPropertyDetail>(context);
            this._ProductBrandRepository = new Repository<ProductBrand>(context);
            this._ProductTypeRepository = new Repository<ProductType>(context);
            this._Context = context;
        }

        public async Task Commit()
        {
            await _Context.SaveChangesAsync();
        }

        public void Rollback()
        {
            if (_Context.Database.CurrentTransaction != null)
            {
                _Context.Database.CurrentTransaction.Rollback();
            }
        }

        public IRepository<ProductPropertyDetail> ProductPropertyDetailRepository()
        {
           return this._ProductPropertyDetailRepository;
        }

        public IRepository<ProductProperty> ProductPropertyRepository()
        {
            return this._ProductPropertyRepository;
        }

        public IRepository<Product> ProductRepository()
        {
            return this._ProductRepository;
        }

        public IRepository<ProductType> ProductTypeRepository()
        {
            return this._ProductTypeRepository;
        }

        public IRepository<ProductBrand> ProductBrandRepository()
        {
           return this._ProductBrandRepository;
        }
    }
}
