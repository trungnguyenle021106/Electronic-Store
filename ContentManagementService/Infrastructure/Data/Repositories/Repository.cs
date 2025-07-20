using Microsoft.EntityFrameworkCore;
using ContentManagementService.Domain.Interface.IRepositories;
using ContentManagementService.Infrastructure.Data.DBContext;

namespace ContentManagementService.Infrastructure.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ContentManagementContext _Context;
        private readonly DbSet<T> _DBSet;

        public Repository(ContentManagementContext _Context)
        {
            this._Context = _Context;
            _DBSet = this._Context.Set<T>();
        }

        public async Task<T> Add(T entity)
        {
            await _DBSet.AddAsync(entity);
            return entity;
        }

        public void Delete(T entity)
        {
            _DBSet.Remove(entity);
        }

        public IQueryable<T> GetAll()
        {
            return _DBSet.AsQueryable();
        }

        public IQueryable<T> GetByIdQueryable(int id)
        {
            return _DBSet.Where(entity => EF.Property<int>(entity, "ID") == id);
        }

        public async Task<T> GetById(int id)
        {
           return await _DBSet.FindAsync(id);
        }

        public IQueryable<T> GetByFieldQueryable<TField>(string fieldName, TField value)
        {
            // Kiểm tra null hoặc chuỗi trống
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("Field name cannot be null or empty.", nameof(fieldName));

            // Sử dụng Entity Framework để tìm kiếm theo trường
            return _DBSet.Where(entity => EF.Property<TField>(entity, fieldName).Equals(value));
        }

        public T Update(T entity)
        {
            _DBSet.Update(entity);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _DBSet.AddRangeAsync(entities);
        }
        public Task RemoveRange(IEnumerable<T> entitiesToDelete)
        {
            _DBSet.RemoveRange(entitiesToDelete);
            return Task.CompletedTask;
        }
    }
}
