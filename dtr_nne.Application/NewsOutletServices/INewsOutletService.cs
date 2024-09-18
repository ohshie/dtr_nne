using dtr_nne.Application.DTO;

namespace dtr_nne.Application.NewsOutletServices;

public interface INewsOutletService
{
    public Task<List<NewsOutletDto>> GetAllNewsOutlets();
    public Task<List<NewsOutletDto>> AddNewsOutlets(List<NewsOutletDto> incomingNewsOutlets);
    public Task<List<NewsOutletDto>> UpdateNewsOutlets(List<NewsOutletDto> incomingNewsOutlets);
}