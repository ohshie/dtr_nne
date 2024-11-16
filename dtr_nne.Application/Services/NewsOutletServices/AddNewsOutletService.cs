using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.Services.NewsOutletServices;

internal class AddNewsOutletService(ILogger<AddNewsOutletService> logger, 
    INewsOutletRepository repository, 
    INewsOutletMapper mapper,
    IUnitOfWork<INneDbContext> unitOfWork) : IAddNewsOutletService
{
    public async Task<List<NewsOutletDto>> AddNewsOutlets(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        if (incomingNewsOutletDtos.Count == 0)
        {
            logger.LogWarning("Provided 0 News Outlets Dto. Returning");
            return [];
        }
        
        logger.LogInformation("Adding {IncomingNewsOutletsDtosCount} Provided News Outlets to Db", incomingNewsOutletDtos.Count);

        var incomingNewsOutlets = mapper.DtosToEntities(incomingNewsOutletDtos);

        var addedNewsOutlets = await repository.AddRange(incomingNewsOutlets);

        if (addedNewsOutlets)
        {
            await unitOfWork.Save();
        }

        var addedNewsOutletsDtos = mapper.EntitiesToDtos(incomingNewsOutlets);
        logger.LogInformation("Added provided News Outlets to Db");
        
        return addedNewsOutletsDtos;
    }
}

public interface IAddNewsOutletService
{
    Task<List<NewsOutletDto>> AddNewsOutlets(List<NewsOutletDto> incomingNewsOutletDtos);
}