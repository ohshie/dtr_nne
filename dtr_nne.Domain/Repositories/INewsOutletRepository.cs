using dtr_nne.Domain.Entities;

namespace dtr_nne.Domain.Repositories;

public interface INewsOutletRepository : IRepository<NewsOutlet>
{
    public bool UpdateRange(IEnumerable<NewsOutlet> incomingNewsOutlets);
}