using dtr_nne.Domain.Entities;

namespace dtr_nne.Application.NewsOutletServices;

public class NewsOutletService() : INewsOutletService
{
    public async Task<List<NewsOutlet>> GetAllNewsOutlets()
    {
        return [];
    }
}