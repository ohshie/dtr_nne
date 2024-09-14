namespace dtr_nne.Domain.UnitOfWork;

public interface IUnitOfWork<out TContext> where TContext : class
{
    TContext Context { get; }
    Task Save();
    void CreateTransaction();
    void Commit();
    void Rollback();
}