using dtr_nne.Application.DTO;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.Services.EntityManager;

public class UpdateManagedEntity<T, TDto>(ILogger<UpdateManagedEntity<T, TDto>> logger,
    IManagedEntityMapper mapper, IManagedEntityHelper<T> helper,
    IRepository<T>  repository, IUnitOfWork<INneDbContext> unitOfWork) : IUpdateManagedEntity<TDto>
    where T : class, IManagedEntity
    where TDto : class, IManagedEntityDto
{
    public async Task<ErrorOr<List<TDto>>> Update(List<TDto> incomingNewsOutletDtos)
    {
        if (incomingNewsOutletDtos.Count == 0)
        {
            logger.LogWarning("Provided 0 News Outlets Dto to update. Returning");
            return Errors.ManagedEntities.NoEntitiesProvided;
        }

        var mappedIncomingNewsOutlets = mapper.DtoToEntity<T, TDto>(incomingNewsOutletDtos);
        
        var matchResult = await helper.MatchEntities(mappedIncomingNewsOutlets);
        if (matchResult.IsError)
        {
            return matchResult.FirstError;
        }

        var (matchedNewsOutlets, notMatchedNewsOutlets) = matchResult.Value;
        if (matchedNewsOutlets.Count < 1)
        {
            logger.LogError("No provided entities found in Db, returning list back to the customer");
            var notMatchedNewsOutletDtos = mapper.EntityToDto<T, TDto>(notMatchedNewsOutlets);
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
            return Errors.ManagedEntities.UpdateFailed(typeof(NewsOutlet));
        }

        await unitOfWork.Save();
        
        if (notMatchedNewsOutlets.Count <  1)
        {
            logger.LogInformation("Successfully updated all provided entities");
            return new List<TDto>();
        }

        logger.LogWarning(
            "Partially updated provided entities, returning {NotMatchedOutletsCount} Dtos for customer to check",
            notMatchedNewsOutlets.Count);
        var notDeletedNewsOutletDtos = mapper.EntityToDto<T, TDto>(notMatchedNewsOutlets); 
        return notDeletedNewsOutletDtos;
    }
}

public interface IUpdateManagedEntity<TDto>
{
    public Task<ErrorOr<List<TDto>>> Update(List<TDto> incomingNewsOutletDtos);
}