using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.Services.NewsOutletServices;

public class UpdateNewsOutletService(ILogger<UpdateNewsOutletService> logger, 
    INewsOutletRepository repository, 
    INewsOutletMapper mapper,
    IUnitOfWork<INneDbContext> unitOfWork) : IUpdateNewsOutletService
{
    public async Task<List<NewsOutletDto>> UpdateNewsOutlets(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        if (incomingNewsOutletDtos.Count == 0)
        {
            logger.LogWarning("Provided 0 News Outlets Dto to update. Returning");
            return [];
        }
        
        logger.LogInformation("Updating {IncomingNewsOutletsDtosCount} Provided News Outlets in Db", incomingNewsOutletDtos.Count);

        var mappedIncomingNewsOutlets = mapper.NewsOutletDtosToNewsOutlets(incomingNewsOutletDtos);
        
        var updatedOutlets = repository.UpdateRange(mappedIncomingNewsOutlets);

        if (updatedOutlets)
        {
            await unitOfWork.Save();
            
            var mappedUpdatedNewsOutlets = mapper.NewsOutletsToNewsOutletsDto(mappedIncomingNewsOutlets);
            logger.LogInformation("Updated {UpdatedNewsOutletsCount} Provided News Outlets to Db", mappedUpdatedNewsOutlets.Count);
            return mappedUpdatedNewsOutlets;
        }
        
        logger.LogWarning("Failed to update provided News Outlets, returning empty list");
        return [];
    }
}

public interface IUpdateNewsOutletService
{
    public Task<List<NewsOutletDto>> UpdateNewsOutlets(List<NewsOutletDto> incomingNewsOutletDtos);
}