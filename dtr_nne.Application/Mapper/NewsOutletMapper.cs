using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Domain.Entities;
using Riok.Mapperly.Abstractions;


namespace dtr_nne.Application.Mapper;

[Mapper]
public partial class NewsOutletMapper : INewsOutletMapper
{
    public partial NewsOutletDto NewsOutletToNewsOutletDto(NewsOutlet newsOutlet);
    public partial List<NewsOutletDto> NewsOutletsToNewsOutletsDto(List<NewsOutlet> newsOutlets);
    
    public partial NewsOutlet NewsOutletDtoToNewsOutlet(NewsOutletDto newsOutletDto);
    public partial List<NewsOutlet> NewsOutletDtosToNewsOutlets(List<NewsOutletDto> newsOutlets);
    public partial List<NewsOutlet> DeleteNewsOutletDtosToNewsOutlet(List<BaseNewsOutletsDto> newsOutletsDtos);
}