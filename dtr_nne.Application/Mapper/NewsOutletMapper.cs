using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Domain.Entities.ManagedEntities;
using Riok.Mapperly.Abstractions;

namespace dtr_nne.Application.Mapper;

[Mapper]
public partial class NewsOutletMapper : INewsOutletMapper
{
    public partial List<NewsOutletDto> EntitiesToDtos(List<NewsOutlet> newsOutlets);

    public partial List<NewsOutlet> DtosToEntities(List<NewsOutletDto> newsOutlets);
}