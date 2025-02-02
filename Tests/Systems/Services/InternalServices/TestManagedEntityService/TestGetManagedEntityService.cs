using dtr_nne.Application.DTO.NewsOutlet;
using dtr_nne.Application.Extensions;
using dtr_nne.Application.Services.EntityManager;
using dtr_nne.Domain.Entities.ManagedEntities;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Fixtures.NewsOutletDtoFixtures;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Systems.Services.InternalServices.TestManagedEntityService;

public class TestGetManagedEntityService : BaseTestManagedEntityService
{
    private readonly GetManagedEntity<NewsOutlet, NewsOutletDto> _sut;

    public TestGetManagedEntityService()
    {
        var logger = new Mock<ILogger<GetManagedEntity<NewsOutlet, NewsOutletDto>>>().Object;

        var randomNewsOutlet = NewsOutletDtoFixtureBase.OutletDtos[0];
        
        MockMapper
            .Setup(mapper => mapper.EntityToDto<NewsOutlet, NewsOutletDto>(It.IsAny<List<NewsOutlet>>()))
            .Returns(randomNewsOutlet);
        
        _sut = new GetManagedEntity<NewsOutlet, NewsOutletDto>(logger: logger, 
            Mockrepository.Object, 
            mapper: MockMapper.Object);
    }
    
    [Fact]
    public async Task GetAll_WhenInvokedEmpty_ReturnEmptyNewsOutletList()
    {
        // Arrange

        // Act
        var newsOutlets = await _sut.GetAll();

        // Assert
        newsOutlets.IsError.Should().BeTrue();
        newsOutlets.FirstError.Should().Be(Errors.ManagedEntities.NotFoundInDb(typeof(NewsOutlet)));
    }
    
    [Theory]
    [ClassData(typeof(NewsOutletFixture))]
    public async Task GetAll_WhenInvokedPopulated_ReturnsNewsOutletList(List<NewsOutlet> incomingNewsOutlet)
    {
        // Arrange
        Mockrepository
            .Setup(repository => repository.GetAll().Result)
            .Returns(incomingNewsOutlet);
        
        // Act
        var newsOutlets = await _sut.GetAll();

        // Assert
        newsOutlets.IsError.Should().BeFalse();
        newsOutlets.Value.Should().BeOfType<List<NewsOutletDto>>();
        newsOutlets.Value.Should().HaveCount(c => c > 0);
    }
}