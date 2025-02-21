﻿using ProductService.Domain.Entities;
using ProductService.Domain.Interface.IRepositories;

namespace ProductService.Domain.Interface.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Product> ProductRepository();
        IRepository<ProductProperty> ProductPropertyRepository();
        IRepository<ProductPropertyDetail> ProductPropertyDetailRepository();
        Task Commit();
        void Rollback();
    }
}
