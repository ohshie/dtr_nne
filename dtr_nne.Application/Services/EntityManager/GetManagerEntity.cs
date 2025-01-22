using dtr_nne.Application.DTO;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Mapper;
using dtr_nne.Domain.Entities.ManagedEntities;
using dtr_nne.Domain.Repositories;

namespace dtr_nne.Application.Services.EntityManager;

public class GetManagerEntity<T, TDto>(ILogger<GetManagerEntity<T, TDto>> logger, 
    IRepository<T> aiAssistantRepository, IManagedEntityMapper mapper) 
    : IGetManagerEntity<TDto>
    where T : class, IManagedEntity
    where TDto : class, IManagedEntityDto
{
    public async Task<ErrorOr<List<TDto>>> GetAll()
    {
        logger.LogInformation("Serving all currently saved openAi assistants");
        var assistants = await aiAssistantRepository.GetAll() as List<T>;
        if (assistants is null)
        {
            return Errors.ManagedEntities.NotFoundInDb(typeof(T));
        }

        var assistantDtos = mapper.EntityToDto<T, TDto>(assistants);
        return assistantDtos;
    }
}

public interface IGetManagerEntity<TDto>
    where TDto : class, IManagedEntityDto
{
    public Task<ErrorOr<List<TDto>>> GetAll();
}