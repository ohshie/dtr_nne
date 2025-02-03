using dtr_nne.Application.DTO.NewsOutlet;

namespace Tests.Fixtures.NewsOutletDtoFixtures;

public class NewsOutletDtoFixture : TheoryData<List<NewsOutletDto>>
{
    public NewsOutletDtoFixture()
    {
        Add(NewsOutletDtoFixtureBase.OutletDtos[0]);
        Add(NewsOutletDtoFixtureBase.OutletDtos[1]);
    }
}