using Microsoft.EntityFrameworkCore;
using BannerService.Domain.Interface.IRepositories;
using BannerService.Infrastructure.DBContext;

namespace BannerService.Infrastructure.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ContentManagementContext _Context;
        public Repository(ContentManagementContext _Context)
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

        public IQueryable<T> GetAll()
        {
            return _Context.Set<T>().AsQueryable();
        }

        public IQueryable<T> GetByIdQueryable(int id)
        {
            return _Context.Set<T>().Where(entity => EF.Property<int>(entity, "ID") == id);
        }

        public async Task<T> GetById(int id)
        {
           return await _Context.Set<T>().FindAsync(id);
        }

        public IQueryable<T> GetByFieldQueryable<TField>(string fieldName, TField value)
        {
            // Kiểm tra null hoặc chuỗi trống
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("Field name cannot be null or empty.", nameof(fieldName));

            // Sử dụng Entity Framework để tìm kiếm theo trường
            return _Context.Set<T>().Where(entity => EF.Property<TField>(entity, fieldName).Equals(value));
        }

        public T Update(T entity)
        {
            _Context.Set<T>().Update(entity);
            return entity;
        }

    }
}
