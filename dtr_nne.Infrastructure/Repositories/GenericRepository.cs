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
    internal DbSet<TEntity> DbSet = unitOfWork.Context.Set<TEntity>();
    
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
        
        return DbSet.FirstOrDefaultAsync(e => EF.Property<int>(e,key) == id);
    }

    public async Task<IEnumerable<TEntity>?> GetAll()
    {
        return await DbSet.AsNoTracking().ToListAsync();
    }

    public async Task<bool> Add(TEntity entity)
    {
        await DbSet.AddAsync(entity);
        return true;
    }

    public async Task<bool> AddRange(IEnumerable<TEntity> entities)
    {
        try
        {
            await DbSet.AddRangeAsync(entities);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Something went really wrong when trying to AddRange to Db {Exception}, \n {ExceptionTrace} \n {ExceptionInnerException}", 
                e.Message, 
                e.StackTrace, 
                e.InnerException?.Message ?? "No Inner Exception");
            return false;
        }
    }

    public bool Update(TEntity entity)
    {
        try
        {
            DbSet.Update(entity);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Something went really wrong when trying to Perform Update in Db {Exception}, \n {ExceptionTrace} \n {ExceptionInnerException}", 
                e.Message, 
                e.StackTrace, 
                e.InnerException?.Message ?? "No Inner Exception");
            return false;
        }
    }
    
    public bool UpdateRange(IEnumerable<TEntity> incomingNewsOutlets)
    {
        try
        {
            unitOfWork.Context.UpdateRange(incomingNewsOutlets);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to pass Update Range on {TypeOfEntity}, Exception: {Exception}, \n" +
                             "{Message}\n" +
                             "{StackTrace}", 
                incomingNewsOutlets.GetType(),
                e.InnerException?.Message ?? "No Inner Exception",
                e.Message,
                e.StackTrace);
            return false;
        }
    }

    public Task<bool> Remove(TEntity? entity)
    {
        throw new NotImplementedException();
    }

    public bool RemoveRange(IEnumerable<TEntity> entities)
    {
        try
        {
            unitOfWork.Context.RemoveRange(entities);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to pass Remove Range on {TypeOfEntity}, " +
                             "Exception: {Exception}, \n" +
                             "{Message}\n" +
                             "{StackTrace}", 
                entities.GetType(),
                e.InnerException?.Message ?? "No Inner Exception",
                e.Message,
                e.StackTrace);
            return false;
        }
    }
}