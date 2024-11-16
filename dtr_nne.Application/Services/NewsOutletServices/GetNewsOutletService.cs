using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.Repositories;

namespace dtr_nne.Application.Services.NewsOutletServices;

public class GetNewsOutletService(ILogger<GetNewsOutletService> logger, 
    INewsOutletRepository repository, 
    INewsOutletMapper mapper) : IGetNewsOutletService
{
    public async Task<List<NewsOutletDto>> GetAllNewsOutlets()
    {
        logger.LogInformation("Serving All Currently Saved News Outlets From Db");
        var newsOutlets = await repository.GetAll() as List<NewsOutlet>;

        if (newsOutlets is null)
        {
            logger.LogWarning("No Saved NEws Outlets Found");
            return [];
        }
        
        var mappedNewsOutlets = mapper.EntitiesToDtos(newsOutlets);
        
        logger.LogInformation("Found {NewsOutletCount} News Outlets, mapped to {MappedNewsOutlets} News Outlets Dto", newsOutlets.Count, mappedNewsOutlets.Count);
        return mappedNewsOutlets;
    }
}

public interface IGetNewsOutletService
{
    Task<List<NewsOutletDto>> GetAllNewsOutlets();
}