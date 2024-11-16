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
    private readonly ILogger<GenericRepository<TEntity, TContext>> Logger = logger;
    
    public Task<TEntity?> Get(int id)
    {
        var key = unitOfWork.Context.Model
            .FindEntityType(typeof(TEntity))?
            .FindPrimaryKey()?.Properties
            .FirstOrDefault()?.Name;
        
        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException($"Entity {typeof(TEntity).Name} does not have a primary key.");
        }
        
        return _dbSet.FirstOrDefaultAsync(e => EF.Property<int>(e,key) == id);
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
        try
        {
            await _dbSet.AddRangeAsync(entities);
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError("Something went really wrong when trying to AddRange to Db {Exception}, \n {ExceptionTrace} \n {ExceptionInnerException}", 
                e.Message, 
                e.StackTrace, 
                e.InnerException?.Message ?? "No Inner Exception");
            throw;
        }
    }

    public bool Update(TEntity entity)
    {
        try
        {
            _dbSet.Update(entity);
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError("Something went really wrong when trying to Perform Update in Db {Exception}, \n {ExceptionTrace} \n {ExceptionInnerException}", 
                e.Message, 
                e.StackTrace, 
                e.InnerException?.Message ?? "No Inner Exception");
            throw;
        }
    }

    public Task<bool> Remove(TEntity? entity)
    {
        throw new NotImplementedException();
    }
}