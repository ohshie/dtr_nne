using dtr_nne.Application.DTO.NewsOutlet;

namespace Tests.Fixtures;

public class BaseNewsOutletsDtoFixture : TheoryData<List<BaseNewsOutletsDto>>
{
    public BaseNewsOutletsDtoFixture()
    {
        Add([
            new BaseNewsOutletsDto
            {
                    Id = Faker.RandomNumber.Next(600), 
                    Name = "arkeonews.net"
            }
        ]);
        Add([
            new BaseNewsOutletsDto
            {
                    Id = Faker.RandomNumber.Next(600),
                    Name = "arkeonews.net"
            },

            new BaseNewsOutletsDto
            {
                    Id = Faker.RandomNumber.Next(600),
                    Name = "eurekalert.org"
            },

            new BaseNewsOutletsDto
            {
                    Id = Faker.RandomNumber.Next(600),
                    Name = "techxplore.com/sort/date/12h/"
            }
        ]);
    }
}