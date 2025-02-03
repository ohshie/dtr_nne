using System.Diagnostics.CodeAnalysis;
using dtr_nne.Application.DTO;
using dtr_nne.Application.DTO.ExternalService.OpenAiAssistantDto;
using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Domain.Entities.ManagedEntities;
using InvalidOperationException = System.InvalidOperationException;

namespace dtr_nne.Application.Mapper;

[SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded")]
public class ManagedEntityMapper : IManagedEntityMapper
{
    public TDto EntityToDto<T, TDto>(T entity) 
        where T : class, IManagedEntity
        where TDto : class, IManagedEntityDto
    {
        return entity switch
        {
            NewsOutlet newsOutlet => MapEntityToDto(newsOutlet) as TDto,
            OpenAiAssistant openAiAssistant => MapEntityToDto(openAiAssistant) as TDto,
            _ => null
        } ?? throw new InvalidOperationException();
    }
    
    public List<TDto> EntityToDto<T, TDto>(List<T> entities) 
        where T : class, IManagedEntity
        where TDto : class, IManagedEntityDto
    {
        return entities switch
        {
            List<NewsOutlet> newsOutlet => MapEntityToDto(newsOutlet) as List<TDto>,
            List<OpenAiAssistant> openAiAssistant => MapEntityToDto(openAiAssistant) as List<TDto>,
            _ => null
        } ?? throw new InvalidOperationException();
    }

    public T DtoToEntity<T, TDto>(TDto entityDto) 
        where T : class, IManagedEntity
        where TDto : class, IManagedEntityDto
    {
        return entityDto switch
        {
            NewsOutletDto newsOutlet => MapDtoToEntitty(newsOutlet) as T,
            OpenAiAssistantDto openAiAssistant => MapDtoToEntitty(openAiAssistant) as T,
            _ => null
        } ?? throw new InvalidOperationException();
    }

    public List<T> DtoToEntity<T, TDto>(List<TDto> entityDtos) 
        where T : class, IManagedEntity
        where TDto : class, IManagedEntityDto
    {
        return entityDtos switch
        {
            List<NewsOutletDto> newsOutlet => MapDtoToEntity(newsOutlet) as List<T>,
            List<BaseNewsOutletsDto> newsOutlet => MapDtoToEntity(newsOutlet) as List<T>,
            List<OpenAiAssistantDto> openAiAssistant => MapDtoToEntity(openAiAssistant) as List<T>,
            _ => null
        } ?? throw new InvalidOperationException();
    }

    internal NewsOutlet MapDtoToEntitty(NewsOutletDto dto)
    {
        return new NewsOutlet
        {
            Id = dto.Id,
            Name = dto.Name,
            Website = dto.Website,
            Themes = dto.Themes,

            MainPagePassword = dto.MainPagePassword,
            NewsPassword = dto.NewsPassword,
            WaitTimer = dto.WaitTime,

            AlwaysJs = dto.AlwaysJs,
            InUse = dto.InUse,
        };
    }
    
    internal NewsOutlet MapDtoToEntitty(BaseNewsOutletsDto dto)
    {
        return new NewsOutlet
        {
            Id = dto.Id,
            Name = dto.Name,
            Website = new Uri("https://empty.com"),
            Themes = [],

            MainPagePassword = string.Empty,
            NewsPassword = string.Empty,
            WaitTimer = string.Empty,

            AlwaysJs = true,
            InUse = true,
        };
    }
    
    internal OpenAiAssistant MapDtoToEntitty(OpenAiAssistantDto dto)
    {
        return new OpenAiAssistant
        {
            Id = dto.Id,
            Role = dto.Role,
            AssistantId = dto.AssistantId
        };
    }
    
    internal List<NewsOutlet> MapDtoToEntity(List<NewsOutletDto> dtos)
    {
        List<NewsOutlet> entities = [];
        entities.AddRange(dtos.Select(MapDtoToEntitty));

        return entities;
    }
    
    internal List<NewsOutlet> MapDtoToEntity(List<BaseNewsOutletsDto> dtos)
    {
        List<NewsOutlet> entities = [];
        entities.AddRange(dtos.Select(MapDtoToEntitty));

        return entities;
    }
    
    internal List<OpenAiAssistant> MapDtoToEntity(List<OpenAiAssistantDto> dtos)
    {
        List<OpenAiAssistant> entities = [];
        entities.AddRange(dtos.Select(MapDtoToEntitty));

        return entities;
    }

    internal NewsOutletDto MapEntityToDto(NewsOutlet entity)
    {
        return new NewsOutletDto
        {
            Id = entity.Id,
            InUse = entity.InUse,
            AlwaysJs = entity.AlwaysJs,
            Website = entity.Website!,
            NewsPassword = entity.NewsPassword,
            Themes = entity.Themes,
            Name = entity.Name,
            MainPagePassword = entity.MainPagePassword,
            WaitTime = entity.WaitTimer
        };
    }
    
    internal OpenAiAssistantDto MapEntityToDto(OpenAiAssistant entity)
    {
        return new OpenAiAssistantDto
        {
            Id = entity.Id,
            Role = entity.Role,
            AssistantId = entity.AssistantId
        };
    }
    
    internal List<NewsOutletDto> MapEntityToDto(List<NewsOutlet> entities)
    {
        List<NewsOutletDto> dtos = [];
        dtos.AddRange(entities.Select(MapEntityToDto));

        return dtos;
    }
    
    internal List<OpenAiAssistantDto> MapEntityToDto(List<OpenAiAssistant> entities)
    {
        List<OpenAiAssistantDto> dtos = [];
        dtos.AddRange(entities.Select(MapEntityToDto));

        return dtos;
    }
}