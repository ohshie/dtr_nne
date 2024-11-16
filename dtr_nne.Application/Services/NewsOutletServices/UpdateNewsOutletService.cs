using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.Services.NewsOutletServices;

public class UpdateNewsOutletService(ILogger<UpdateNewsOutletService> logger, 
    INewsOutletServiceHelper helper,
    INewsOutletRepository repository, 
    INewsOutletMapper mapper,
    IUnitOfWork<INneDbContext> unitOfWork) : IUpdateNewsOutletService
{
    public async Task<ErrorOr<List<NewsOutletDto>>> UpdateNewsOutlets(List<NewsOutletDto> incomingNewsOutletDtos)
    {
        if (incomingNewsOutletDtos.Count == 0)
        {
            logger.LogWarning("Provided 0 News Outlets Dto to update. Returning");
            return Errors.NewsOutlets.NoNewsOutletProvided;
        }

        var mappedIncomingNewsOutlets = mapper.DtosToEntities(incomingNewsOutletDtos);
        
        var matchResult = await helper.MatchNewsOutlets(mappedIncomingNewsOutlets);
        if (matchResult.IsError)
        {
            return matchResult.FirstError;
        }

        var (matchedNewsOutlets, notMatchedNewsOutlets) = matchResult.Value;
        if (matchedNewsOutlets.Count < 1)
        {
            logger.LogError("No provided entities found in Db, returning list back to the customer");
            var notMatchedNewsOutletDtos = mapper.EntitiesToDtos(notMatchedNewsOutlets);
            return notMatchedNewsOutletDtos;
        }

        var pendingUpdate = mappedIncomingNewsOutlets
            .Where(ino => matchedNewsOutlets
                .Select(no => no.Id)
                .Contains(ino.Id))
            .ToList();
        
        var success = repository.UpdateRange(pendingUpdate);
        if (!success)
        {
            logger.LogError("Updating range of entities from Db resulted in Error");
            return Errors.NewsOutlets.UpdateFailed;
        }

        await unitOfWork.Save();
        
        if (notMatchedNewsOutlets.Count <  1)
        {
            logger.LogInformation("Successfully updated all provided entities");
            return new List<NewsOutletDto>();
        }

        logger.LogWarning(
            "Partially updated provided entities, returning {NotMatchedOutletsCount} Dtos for customer to check",
            notMatchedNewsOutlets.Count);
        var notDeletedNewsOutletDtos = mapper.EntitiesToDtos(notMatchedNewsOutlets); 
        return notDeletedNewsOutletDtos;
    }
}

public interface IUpdateNewsOutletService
{
    public Task<ErrorOr<List<NewsOutletDto>>> UpdateNewsOutlets(List<NewsOutletDto> incomingNewsOutletDtos);
}