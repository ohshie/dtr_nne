using dtr_nne.Application.DTO;

namespace Tests.Fixtures;

public class DeleteNewsOutletsDtoFixture : TheoryData<List<DeleteNewsOutletsDto>>
{
    public DeleteNewsOutletsDtoFixture()
    {
        Add([
            new DeleteNewsOutletsDto
            {
                    Id = Faker.RandomNumber.Next(300), 
                    Name = "arkeonews.net"
            }
        ]);
        Add([
            new DeleteNewsOutletsDto
            {
                    Id = Faker.RandomNumber.Next(300),
                    Name = "arkeonews.net"
            },

            new DeleteNewsOutletsDto
            {
                    Id = Faker.RandomNumber.Next(300),
                    Name = "eurekalert.org"
            },

            new DeleteNewsOutletsDto
            {
                    Id = Faker.RandomNumber.Next(300),
                    Name = "techxplore.com/sort/date/12h/"
            }
        ]);
    }
}