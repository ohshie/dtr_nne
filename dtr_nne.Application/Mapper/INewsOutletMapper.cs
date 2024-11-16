using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Domain.Entities;

namespace dtr_nne.Application.Mapper;

public interface INewsOutletMapper
{
    public NewsOutletDto NewsOutletToNewsOutletDto(NewsOutlet newsOutlet);
    public List<NewsOutletDto> NewsOutletsToNewsOutletsDto(List<NewsOutlet> newsOutlets);
    
    public NewsOutlet NewsOutletDtoToNewsOutlet(NewsOutletDto newsOutletDto);
    public List<NewsOutlet> NewsOutletDtosToNewsOutlets(List<NewsOutletDto> newsOutlets);
    public List<NewsOutlet> DeleteNewsOutletDtosToNewsOutlet(List<BaseNewsOutletsDto> newsOutletsDtos);
}