using dtr_nne.Application.DTO;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.IContext;
using dtr_nne.Domain.Repositories;
using dtr_nne.Domain.UnitOfWork;

namespace dtr_nne.Application.Services.EntityManager;

public class AddManagedEntity<T, TDto>(ILogger<AddManagedEntity<T, TDto>> logger,
    IRepository<T> repository, IUnitOfWork<INneDbContext> unitOfWork,
    IManagedEntityMapper mapper) 
    : IAddManagedEntity<TDto>
    where T : class, IManagedEntity
    where TDto : class, IManagedEntityDto
{
    public async Task<ErrorOr<List<TDto>>> Add(List<TDto> entityDtos)
    {
        if (entityDtos.Count < 1)
        {
            logger.LogWarning("Provided 0 entitiesDto to delete. Returning");
            return Errors.ManagedEntities.NoEntitiesProvided;
        }

        var mappedIncomingEntity = mapper.DtoToEntity<T, TDto>(entityDtos);
        
        logger.LogInformation("Adding {IncomingEntities} entities to db", entityDtos.Count);

        var success = await repository.AddRange(mappedIncomingEntity);
        if (success)
        {
            await unitOfWork.Save();
        }
        
        logger.LogInformation("Added provided open ai assistants to Db");
        
        var addedOpenAiAssistantDtos = mapper.EntityToDto<T, TDto>(mappedIncomingEntity);
        return addedOpenAiAssistantDtos;
    }
}

public interface IAddManagedEntity<TDto>
    where TDto : class, IManagedEntityDto
{
    public Task<ErrorOr<List<TDto>>> Add(List<TDto> entityDtos);
}