using dtr_nne.Application.DTO;
using dtr_nne.Domain.Entities;

namespace dtr_nne.Application.NewsOutletServices;

public interface INewsOutletService
{
    public Task<List<NewsOutletDto>> GetAllNewsOutlets();
    public Task<List<NewsOutletDto>> AddNewsOutlets(List<NewsOutletDto> incomingNewsOutlets);
}