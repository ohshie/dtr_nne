using dtr_nne.Domain.Entities.ManagedEntities;

namespace Tests.Fixtures.NewsOutletFixtures;

public class NewsOutletFixture : TheoryData<List<NewsOutlet>>
{
    public NewsOutletFixture()
    {
        Add(NewsOutletFixtureBase.Outlets[0]);
        Add(NewsOutletFixtureBase.Outlets[1]);
    }
}