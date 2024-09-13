using Microsoft.EntityFrameworkCore;

namespace dtr_nne.Domain.UnitOfWork;

public interface IUnitOfWork<out TContext> where TContext : DbContext
{
    TContext Context { get; }
    Task Save();
    void CreateTransaction();
    void Commit();
    void Rollback();
}