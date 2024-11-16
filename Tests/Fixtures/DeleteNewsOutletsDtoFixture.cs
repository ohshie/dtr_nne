using dtr_nne.Application.DTO.NewsOutlet;

namespace Tests.Fixtures;

public class DeleteNewsOutletsDtoFixture : TheoryData<List<BaseNewsOutletsDto>>
{
    public DeleteNewsOutletsDtoFixture()
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