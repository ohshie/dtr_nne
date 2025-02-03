using dtr_nne.Application.DTO;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Repositories;

namespace dtr_nne.Application.Services.EntityManager;

public class GetManagedEntity<T, TDto>(ILogger<GetManagedEntity<T, TDto>> logger, 
    IRepository<T> aiAssistantRepository, IManagedEntityMapper mapper) 
    : IGetManagedEntity<TDto>
    where T : class, IManagedEntity
    where TDto : class, IManagedEntityDto
{
    public async Task<ErrorOr<List<TDto>>> GetAll()
    {
        logger.LogInformation("Serving all currently saved openAi assistants");
        if (await aiAssistantRepository.GetAll() is not List<T> assistants || assistants.Count < 1)
        {
            return Errors.ManagedEntities.NotFoundInDb(typeof(T));
        }

        var assistantDtos = mapper.EntityToDto<T, TDto>(assistants);
        return assistantDtos;
    }
}

public interface IGetManagedEntity<TDto>
    where TDto : class, IManagedEntityDto
{
    public Task<ErrorOr<List<TDto>>> GetAll();
}