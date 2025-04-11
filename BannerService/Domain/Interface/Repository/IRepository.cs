namespace BannerService.Domain.Interface.IRepositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> GetByIdQueryable(int id);
        Task<TEntity> GetById(int id);
        Task<TEntity> Add(TEntity entity);
        TEntity Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
