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
    
    public async Task<bool> Save()
    {
        try
        {
            await Context.SaveChangesAsync();
            return true;
        }
        catch (Exception dbEx)
        {
            throw;
            return false;
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
            if (disposing)
                Context.Dispose();
        _disposed = true;
    }
}