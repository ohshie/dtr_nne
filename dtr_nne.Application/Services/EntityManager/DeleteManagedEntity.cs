using System.Runtime.CompilerServices;
using dtr_nne.Application.DTO;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

[assembly: InternalsVisibleTo("Tests")]
namespace dtr_nne.Application.Services.EntityManager;

public class DeleteManagedEntity<T, TDto>(ILogger<DeleteManagedEntity<T, TDto>> logger, 
    IManagedEntityMapper mapper, IManagedEntityHelper<T> helper,
    IRepository<T> repository, IUnitOfWork<INneDbContext> unitOfWork) 
    : IDeleteManagedEntity<TDto> 
    where T : class, IManagedEntity
    where TDto : class, IManagedEntityDto
{
    public async Task<ErrorOr<List<TDto>>> Delete(List<TDto> dtos)
    {
        if (dtos.Count == 0)
        {
            logger.LogWarning("Provided 0 entitiesDto to delete. Returning");
            return Errors.ManagedEntities.NoEntitiesProvided;
        }

        var mappedEntities = mapper.DtoToEntity<T, TDto>(dtos);

        var processedEntities = await helper.MatchEntities(mappedEntities);
        if (processedEntities.IsError)
        {
            return processedEntities.FirstError;
        }

        var (matchedEntities, notmatchedEntities) = processedEntities.Value;
        if (notmatchedEntities.Count < 1)
        {
            logger.LogWarning("No provided entities found in Db, returning list back to the customer");
            var notMatchedNewsOutletDtos = mapper.EntityToDto<T, TDto>(notmatchedEntities);
            return notMatchedNewsOutletDtos;
        }

        var success = repository.RemoveRange(matchedEntities);
        if (!success)
        {
            logger.LogError("Removing range of entities from Db resulted in Error");
            return Errors.ManagedEntities.DeletionFailed(typeof(NewsOutlet));
        }

        await unitOfWork.Save();

        if (notmatchedEntities.Count < 1)
        {
            logger.LogInformation("Successfully removed all provided entities from Db");
            return new List<TDto> { Capacity = 0 };
        }

        logger.LogWarning(
            "Partially removed provided entities from Db, returning {NotMatchedOutletsCount} Dtos for customer to check",
            notmatchedEntities.Count);
        var notDeletedNewsOutletDtos = mapper.EntityToDto<T, TDto>(notmatchedEntities);
        return notDeletedNewsOutletDtos;
    }
}

public interface IDeleteManagedEntity<TDto>
    where TDto : class, IManagedEntityDto
{
    public Task<ErrorOr<List<TDto>>> Delete(List<TDto> dtos);
}