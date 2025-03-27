﻿using Microsoft.EntityFrameworkCore;
using BannerService.Domain.Interface.IRepositories;
using BannerService.Infrastructure.DBContext;

namespace BannerService.Infrastructure.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly BannerContext _Context;
        public Repository(BannerContext _Context)
        {
            this._Context = _Context;
        }

        public async Task<T> Add(T entity)
        {
            await _Context.Set<T>().AddAsync(entity);
            return entity;
        }

        public void Delete(T entity)
        {
            _Context.Set<T>().Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _Context.Set<T>().ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
           return await _Context.Set<T>().FindAsync(id);
        }

        public T Update(T entity)
        {
            _Context.Set<T>().Update(entity);
            return entity;
        }
    }
}
