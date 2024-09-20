using System.Runtime.CompilerServices;
using dtr_nne.Application.DTO;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;
using ErrorOr;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Tests")]

namespace dtr_nne.Application.NewsOutletServices;

public class NewsOutletService(ILogger<NewsOutletService> logger, 
    INewsOutletRepository repository, 
    INewsOutletMapper mapper,
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

    public async Task<List<NewsOutletDto>> AddNewsOutlets(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        if (incomingNewsOutletDtos.Count == 0)
        {
            logger.LogWarning("Provided 0 News Outlets Dto. Returning");
            return [];
        }
        
        logger.LogInformation("Adding {IncomingNewsOutletsDtosCount} Provided News Outlets to Db", incomingNewsOutletDtos.Count);

        var incomingNewsOutlets = mapper.NewsOutletDtosToNewsOutlets(incomingNewsOutletDtos);

        var addedNewsOutlets = await repository.AddRange(incomingNewsOutlets);

        if (addedNewsOutlets)
        {
            await unitOfWork.Save();
        }

        var addedNewsOutletsDtos = mapper.NewsOutletsToNewsOutletsDto(incomingNewsOutlets);
        logger.LogInformation("Added {AddedNewsOutletsCount} Provided News Outlets to Db", addedNewsOutletsDtos.Count);
        
        return addedNewsOutletsDtos;
    }

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

    public async Task<ErrorOr<List<NewsOutletDto>>> DeleteNewsOutlets(List<DeleteNewsOutletsDto> incomingNewsOutletDtos)
    {
        if (incomingNewsOutletDtos.Count == 0)
        {
            logger.LogWarning("Provided 0 News Outlets Dto to delete. Returning");
            return Errors.NewsOutlets.NoNewsOutletProvided;
        }

        var mappedIncomingNewsOutlets = mapper.DeleteNewsOutletDtosToNewsOutlet(incomingNewsOutletDtos);

        var savedNewsOutlets = await repository.GetAll() as List<NewsOutlet>;
        
        if (savedNewsOutlets is null)
        {
            logger.LogError("Currently there are no saved NewsOutlets in Db");
            return Errors.NewsOutlets.NotFoundInDb;
        }
        
        var matchedNewsOutlets = savedNewsOutlets
            .Where(mino => 
                mappedIncomingNewsOutlets
                    .Select(no => no.Id)
                    .Contains(mino.Id))
            .ToList();
        
        var notMatchedNewsOutlets = mappedIncomingNewsOutlets
                .Where(mino => 
                    !savedNewsOutlets
                        .Select(no => no.Id)
                        .Contains(mino.Id))
                .ToList();

        var success = repository.RemoveRange(matchedNewsOutlets);
        if (!success)
        {
            return Errors.NewsOutlets.DeletionFailed;
        }

        await unitOfWork.Save();
        
        var notDeletedNewsOutletDtos = mapper.NewsOutletsToNewsOutletsDto(notMatchedNewsOutlets); 
        
        return notDeletedNewsOutletDtos;
    }
}