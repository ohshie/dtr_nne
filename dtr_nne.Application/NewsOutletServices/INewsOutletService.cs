using dtr_nne.Domain.Entities;

namespace dtr_nne.Application.NewsOutletServices;

public interface INewsOutletService
{
    public Task<List<NewsOutlet>> GetAllNewsOutlets();
}