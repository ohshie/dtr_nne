using dtr_nne.Application.DTO;
using dtr_nne.Domain.Entities.ManagedEntities;

namespace dtr_nne.Application.Mapper;

public interface IManagedEntityMapper
{
    public TDto EntityToDto<T, TDto>(T entity) 
        where T : class, IManagedEntity
        where TDto : class, IManagedEntityDto;

    public List<TDto> EntityToDto<T, TDto>(List<T> entities)
        where T : class, IManagedEntity
        where TDto : class, IManagedEntityDto;

    public T DtoToEntity<T, TDto>(TDto entityDto) 
        where T : class, IManagedEntity
        where TDto : class, IManagedEntityDto;

    public List<T> DtoToEntity<T, TDto>(List<TDto> entityDtos)
        where T : class, IManagedEntity
        where TDto : class, IManagedEntityDto;
}