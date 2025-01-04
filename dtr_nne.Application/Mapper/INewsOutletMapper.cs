using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Domain.Entities;

namespace dtr_nne.Application.Mapper;

public interface INewsOutletMapper
{
    public List<NewsOutletDto> EntitiesToDtos(List<NewsOutlet> newsOutlets);
    public List<NewsOutlet> DtosToEntities(List<NewsOutletDto> newsOutlets);
    public List<NewsOutlet> BaseDtosToEntities(List<BaseNewsOutletsDto> newsOutletsDtos);
    public List<BaseNewsOutletsDto> EntitiesToBaseDtos(List<NewsOutlet> newsOutlets);
}