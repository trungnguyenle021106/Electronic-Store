using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.IRepositories;
using ProductService.Infrastructure.Data.DBContext;
using System.Linq.Expressions;
using System.Reflection;

namespace ProductService.Infrastructure.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ProductContext _Context;
        private readonly DbSet<T> _DBSet;

        public Repository(ProductContext _Context)
        {
            this._Context = _Context;
            _DBSet = this._Context.Set<T>();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _DBSet.AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _DBSet.AddRangeAsync(entities);
        }

        public void Remove(T entity)
        {
            _DBSet.Remove(entity);
        }

        public IQueryable<T> GetAll()
        {
            return _DBSet.AsQueryable();
        }

        public IQueryable<T> GetEntitiesByForeignKeyId(int ID, string nameColumn)
        {
            return _DBSet.Where(e => EF.Property<int>(e, nameColumn) == ID);
        }

        public async Task<T> GetById(int id)
        {
            return await _DBSet.FindAsync(id);
        }

        public IQueryable<T> GetByIdQueryable(int id)
        {
            return _DBSet.Where(entity => EF.Property<int>(entity, "ID") == id);
        }

        public IQueryable<T> GetByNameQueryable(string name)
        {
            return _DBSet.Where(entity => EF.Property<string>(entity, "Name") == name);
        }

        public T Update(T entity)
        {
            _DBSet.Update(entity);
            return entity;
        }

        public IQueryable<T> GetByCompositeKey(int id1, string keyProperty1Name, int id2, string keyProperty2Name)
        {
            if (string.IsNullOrWhiteSpace(keyProperty1Name))
            {
                throw new ArgumentException("Tên thuộc tính khóa thứ nhất không được để trống.", nameof(keyProperty1Name));
            }
            if (string.IsNullOrWhiteSpace(keyProperty2Name))
            {
                throw new ArgumentException("Tên thuộc tính khóa thứ hai không được để trống.", nameof(keyProperty2Name));
            }

            // 1. Tạo tham số biểu thức cho entity (e => ...)
            var parameter = Expression.Parameter(typeof(T), "e");

            // 2. Xây dựng điều kiện cho thuộc tính khóa thứ nhất
            Expression property1Access = Expression.Property(parameter, keyProperty1Name);
            if (property1Access.Type != typeof(int))
            {
                throw new ArgumentException($"Thuộc tính '{keyProperty1Name}' không phải là kiểu int.", nameof(keyProperty1Name));
            }
            Expression constant1 = Expression.Constant(id1, typeof(int));
            BinaryExpression condition1 = Expression.Equal(property1Access, constant1);

            // 3. Xây dựng điều kiện cho thuộc tính khóa thứ hai
            Expression property2Access = Expression.Property(parameter, keyProperty2Name);
            if (property2Access.Type != typeof(int))
            {
                throw new ArgumentException($"Thuộc tính '{keyProperty2Name}' không phải là kiểu int.", nameof(keyProperty2Name));
            }
            Expression constant2 = Expression.Constant(id2, typeof(int));
            BinaryExpression condition2 = Expression.Equal(property2Access, constant2);

            // 4. Kết hợp hai điều kiện bằng phép toán AND
            BinaryExpression combinedCondition = Expression.AndAlso(condition1, condition2);

            // 5. Tạo biểu thức lambda hoàn chỉnh (e => condition1 && condition2)
            Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(combinedCondition, parameter);

            // 6. Áp dụng predicate vào DbSet và trả về IQueryable
            return _DBSet.Where(predicate);
        }

        public async Task<List<T>?> RemoveRange(IEnumerable<T> entitiesToDelete)
        {
            if (entitiesToDelete == null || !entitiesToDelete.Any())
            {
                return null;
            }

            PropertyInfo? idProperty = typeof(T).GetProperty("ID", BindingFlags.Public | BindingFlags.Instance);
            if (idProperty == null || idProperty.PropertyType != typeof(int))
            {
                throw new InvalidOperationException(
                    $"This entity '{typeof(T).Name}' have not 'ID' field." +
                    " This RemoveRange method only support enntity have 'ID' field."
                );
            }

            var ids = entitiesToDelete.Select(e => (int)idProperty.GetValue(e)!).ToList();
            if (!ids.Any()) return null;


            var existingEntities = await _DBSet.Where(e => ids.Contains((int)idProperty.GetValue(e)!))
                                                .ToListAsync();

            if (!existingEntities.Any())
            {
                return new List<T>();
            }

            _DBSet.RemoveRange(existingEntities);

            return existingEntities;

        }
    }
}
