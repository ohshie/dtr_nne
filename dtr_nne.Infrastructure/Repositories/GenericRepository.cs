using System.Runtime.CompilerServices;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace dtr_nne.Infrastructure.Repositories;

internal class GenericRepository<TEntity, TContext>(
    ILogger<GenericRepository<TEntity, TContext>> logger,
    IUnitOfWork<TContext> unitOfWork) : IRepository<TEntity> 
    where TEntity : class
    where TContext : DbContext
{
    private readonly DbSet<TEntity> _dbSet = unitOfWork.Context.Set<TEntity>();
    protected readonly ILogger<GenericRepository<TEntity, TContext>> Logger = logger;
    
    public Task<TEntity> Get(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<TEntity>?> GetAll()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<bool> Add(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        return true;
    }

    public async Task<bool> AddRange(IEnumerable<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        return true;
    }

    public Task<bool> Update(TEntity? entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Remove(TEntity? entity)
    {
        throw new NotImplementedException();
    }
}