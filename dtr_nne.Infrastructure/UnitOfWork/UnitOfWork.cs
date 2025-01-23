using dtr_nne.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace dtr_nne.Infrastructure.UnitOfWork;

internal class UnitOfWork<TContext>(TContext passedContext, ILogger<UnitOfWork<TContext>> logger)
    : IUnitOfWork<TContext>, IDisposable where TContext : DbContext
{
    public TContext Context { get; } = passedContext;
    private bool _disposed;
    
    public async Task<bool> Save()
    {
        try
        {
            await Context.SaveChangesAsync();
            return true;
        }
        catch (Exception dbEx)
        {
            logger.LogCritical(dbEx, "Unit Of Work saving produced Error {DbException}", dbEx.Message);
            return false;
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing) Context.Dispose();
        _disposed = true;
    }
}