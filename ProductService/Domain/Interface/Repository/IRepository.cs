namespace ProductService.Domain.Interface.IRepositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> GetByIdQueryable(int id);
        IQueryable<TEntity> GetEntitiesByForeignKeyId(int ID, string nameColumn);
        Task<TEntity> GetById(int id);
        Task<TEntity> AddAsync(TEntity entity);
        TEntity Update(TEntity entity);
        void Remove(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        IQueryable<TEntity> GetByCompositeKey(int id1, string keyProperty1Name, int id2, string keyProperty2Name);
        Task RemoveRange(IEnumerable<TEntity> entitiesToDelete);

    }
}
