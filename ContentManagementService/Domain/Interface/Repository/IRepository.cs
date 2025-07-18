namespace ContentManagementService.Domain.Interface.IRepositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> GetByIdQueryable(int id);
        IQueryable<TEntity> GetByFieldQueryable<TField>(string fieldName, TField value);
        Task<TEntity> GetById(int id);
        Task<TEntity> Add(TEntity entity);
        TEntity Update(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        void Delete(TEntity entity);
    }
}
