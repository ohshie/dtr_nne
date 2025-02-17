using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Domain.Entities.ManagedEntities;

namespace dtr_nne.Application.Mapper;

public interface INewsOutletMapper
{
    public List<NewsOutletDto> EntitiesToDtos(List<NewsOutlet> newsOutlets);
    public List<NewsOutlet> DtosToEntities(List<NewsOutletDto> newsOutlets);
}