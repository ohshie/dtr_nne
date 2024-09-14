using dtr_nne.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace dtr_nne.Infrastructure.UnitOfWork;

internal class UnitOfWork<TContext>(TContext passedContext)
    : IUnitOfWork<TContext>, IDisposable where TContext : DbContext
{
    public TContext Context { get; } = passedContext;
    private bool _disposed;
    private IDbContextTransaction _objTran;
    
    public async Task Save()
    {
        try
        {
            await Context.SaveChangesAsync();
        }
        catch (Exception dbEx)
        {
            throw;
        }
    }

    public void CreateTransaction()
    {
        _objTran = Context.Database.BeginTransaction();
    }

    public void Commit()
    {
        _objTran.Commit();
    }

    public void Rollback()
    {
        _objTran.Rollback();
        _objTran.Dispose();
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
            if (disposing)
                Context.Dispose();
        _disposed = true;
    }
}