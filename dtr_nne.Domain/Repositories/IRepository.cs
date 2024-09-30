namespace dtr_nne.Domain.Repositories;

public interface IRepository<TEntity>
    where TEntity : class
{
    Task<TEntity?> Get(int id);
    Task<IEnumerable<TEntity>?> GetAll();
    Task<bool> Add(TEntity entity);
    public Task<bool> AddRange(IEnumerable<TEntity> entities);
    bool Update(TEntity entity);
    Task<bool> Remove(TEntity entity);
}