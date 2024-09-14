using System.Runtime.CompilerServices;
using dtr_nne.Application.DTO;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Tests")]

namespace dtr_nne.Application.NewsOutletServices;

public class NewsOutletService(ILogger<NewsOutletService> logger, 
    IRepository<NewsOutlet> repository, 
    NewsOutletMapper mapper,
    IUnitOfWork<INneDbContext> unitOfWork) : INewsOutletService
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
        
        var mappedNewsOutlets = mapper.NewsOutletsToNewsOutletsDto(newsOutlets);
        
        logger.LogInformation("Found {NewsOutletCount} News Outlets, mapped to {MappedNewsOutlets} News Outlets Dto", newsOutlets.Count, mappedNewsOutlets.Count);
        return mappedNewsOutlets;
    }

    public async Task<List<NewsOutletDto>> AddNewsOutlets(List<NewsOutletDto> incomingNewsOutletsDtos)
    {
        if (incomingNewsOutletsDtos.Count == 0)
        {
            logger.LogWarning("Provided 0 News Outlets Dto. Returning");
            return [];
        }
        
        logger.LogInformation("Adding {IncomingNewsOutletsDtosCount} Provided News Outlets to Db", incomingNewsOutletsDtos.Count);

        var incomingNewsOutlets = mapper.NewsOutletDtosToNewsOutlets(incomingNewsOutletsDtos);

        var addedNewsOutlets = await repository.AddRange(incomingNewsOutlets);

        if (addedNewsOutlets)
        {
            await unitOfWork.Save();
        }

        var addedNewsOutletsDtos = mapper.NewsOutletsToNewsOutletsDto(incomingNewsOutlets);
        logger.LogInformation("Added {IncomingNewsOutletsCount} Provided News Outlets to Db", incomingNewsOutlets.Count);
        
        return addedNewsOutletsDtos;
    }
}