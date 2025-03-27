using Microsoft.EntityFrameworkCore;
using UserService.Domain.Interface.IRepositories;
using UserService.Infrastructure.DBContext;

namespace UserService.Infrastructure.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly UserContext _Context;
        public Repository(UserContext _Context)
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
