namespace dtr_nne.Domain.UnitOfWork;

public interface IUnitOfWork<out TContext> where TContext : class
{
    TContext Context { get; }
    Task<bool> Save();
}