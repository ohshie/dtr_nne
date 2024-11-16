using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Domain.Entities;
using Riok.Mapperly.Abstractions;


namespace dtr_nne.Application.Mapper;

[Mapper]
public partial class NewsOutletMapper : INewsOutletMapper
{
    public partial NewsOutletDto EntityToDto(NewsOutlet newsOutlet);
    public partial List<NewsOutletDto> EntitiesToDtos(List<NewsOutlet> newsOutlets);
    
    public partial NewsOutlet DtoToEntity(NewsOutletDto newsOutletDto);
    public partial List<NewsOutlet> DtosToEntities(List<NewsOutletDto> newsOutlets);
    public partial List<NewsOutlet> BaseDtosToEntities(List<BaseNewsOutletsDto> newsOutletsDtos);
    public partial List<BaseNewsOutletsDto> EntitiesToBaseDtos(List<NewsOutlet> newsOutlets);
}